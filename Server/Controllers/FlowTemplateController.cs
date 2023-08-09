﻿using System.Text.RegularExpressions;
using FileFlows.Server.Helpers;
using FileFlows.Shared.Helpers;
using FileFlows.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileFlows.Server.Controllers;


/// <summary>
/// Controller for Flow Templates
/// </summary>
[Route("/api/flow-template")]
public class FlowTemplateController : Controller
{
    const int DEFAULT_XPOS = 450;
    const int DEFAULT_YPOS = 50;
    private static List<FlowTemplateModel> Templates;
    private static DateTime FetchedAt = DateTime.MinValue;
    private static readonly string FlowTemplatesFile = Path.Combine(DirectoryHelper.ConfigDirectory, "flow-templates.json");
    
    /// <summary>
    /// Gets all the flow templates
    /// </summary>
    /// <returns>all the flow templates</returns>
    [HttpGet]
    public async Task<List<FlowTemplateModel>> GetAll()
    {
        if (Templates == null || FetchedAt < DateTime.Now.AddMinutes(-10))
            await RefreshTemplates();
        return Templates ?? new();
    }

    /// <summary>
    /// Fetches a flow template
    /// </summary>
    /// <param name="model">the flow to fetch</param>
    /// <returns>the flow</returns>
    [HttpPost]
    public async Task<IActionResult> FetchTemplate([FromBody] FlowTemplateModel model)
    {
        string json;
        string fileName = Path.Combine(DirectoryHelper.TemplateDirectoryFlow, MakeSafeFilename(model.Path.Replace(".json", "_" + model.Revision + ".json")));
        if (System.IO.File.Exists(fileName) == false)
        {
            // need to download the flow template from github
            var result = await HttpHelper.Get<string>("https://raw.githubusercontent.com/revenz/FileFlowsRepository/master/" + model.Path);
            if (result.Success == false)
                return BadRequest("Failed to download from repository");
            await System.IO.File.WriteAllTextAsync(fileName, result.Data);
            json = result.Data;
        }
        else
        {
            json = await System.IO.File.ReadAllTextAsync(fileName);
        }
        
        var elements = await FlowController.GetFlowElements((FlowType)(-1)); // special case to load all template types
        var parts = elements.ToDictionary(x => x.Uid, x => x);
        var fresult = GetFlowTemplate(parts, json);
        model.Fields = fresult.Template.Fields;
        model.Parts = fresult.Flow.Parts;
        model.Flow = fresult.Flow;
        return Ok(model);
    }

    /// <summary>
    /// Refreshes the templates
    /// </summary>
    async Task RefreshTemplates()
    {
        var result = await HttpHelper.Get<List<FlowTemplateModel>>(
            "https://raw.githubusercontent.com/revenz/FileFlowsRepository/master/flows.json?dt=" +
            DateTime.UtcNow.Ticks);
        
        if (result.Success == false)
        {
            // try loading from disk
            LoadFlowTemplatesFromLocalStorage();
        }
        else
        {
            Templates = result.Data;
            FetchedAt = DateTime.Now;

            // save to disk
            string json = JsonSerializer.Serialize(result.Data);
            _ = System.IO.File.WriteAllTextAsync(FlowTemplatesFile, json);
        }
        Templates = Templates.OrderBy(x => x.Name.IndexOf(" ", StringComparison.Ordinal) < 0 ? 1 : Regex.IsMatch(x.Name, "^[\\w]+ File") ? 2 : 3)
            .ThenBy(x => x.Name)
            .ToList();
    }

    /// <summary>
    /// Loads the flows templates from the local storage
    /// </summary>
    private void LoadFlowTemplatesFromLocalStorage()
    {
        if(System.IO.File.Exists(FlowTemplatesFile) == false)
            return;
        try
        {
            string json = System.IO.File.ReadAllText(FlowTemplatesFile);
            Templates = JsonSerializer.Deserialize<List<FlowTemplateModel>>(json);
            FetchedAt = DateTime.Now;
        }
        catch (Exception ex)
        {
            Logger.Instance.WLog("Error parsing flow-templates.json, deleting:  " + ex.Message);
            System.IO.File.Delete(FlowTemplatesFile);
        }
    }
    
    /// <summary>
    /// Makes a string safe for use as a filename by removing or replacing invalid characters.
    /// </summary>
    /// <param name="input">The input string representing the desired filename.</param>
    /// <returns>A safe filename with invalid characters replaced by underscores.</returns>
    static string MakeSafeFilename(string input)
    {
        // Remove or replace invalid characters
        string invalidCharsRegex = string.Join("", Path.GetInvalidFileNameChars());
        string safeFilename = Regex.Replace(input, "[" + Regex.Escape(invalidCharsRegex) + "]", "_");

        return safeFilename;
    }


    private (bool Success, FlowTemplate? Template, Flow Flow) GetFlowTemplate(Dictionary<string, FlowElement> parts, string json)
    {
        try
        {
            if (json.StartsWith("//"))
            {
                json = string.Join("\n", json.Split('\n').Skip(1)).Trim();
            }

            for (int i = 1; i < 50; i++)
            {
                Guid oldUid = new Guid("00000000-0000-0000-0000-0000000000" + (i < 10 ? "0" : "") + i);
                Guid newUid = Guid.NewGuid();
                json = json.Replace(oldUid.ToString(), newUid.ToString());
            }

            json = TemplateHelper.ReplaceWindowsPathIfWindows(json);
            FlowTemplate jst;
            if (json.Contains("\"node\"") == false)
            {
                // "node" is legacy
                jst = LoadModernTemplate(json);
            }
            else
            {
                jst = JsonSerializer.Deserialize<FlowTemplate>(json, new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true
                });
                jst.Author = "FileFlows";
            }

            if (jst == null)
                return (false, null, null);

            try
            {

                List<FlowPart> flowParts = new ();
                int y = DEFAULT_YPOS;
                bool invalid = false;
                foreach (var jsPart in jst.Parts)
                {
                    if (jsPart.Node == null || parts.ContainsKey(jsPart.Node) == false)
                    {
                        invalid = true;
                        break;
                    }

                    var element = parts[jsPart.Node];

                    flowParts.Add(new FlowPart
                    {
                        yPos = jsPart.yPos ?? y,
                        xPos = jsPart.xPos ?? DEFAULT_XPOS,
                        FlowElementUid = element.Uid,
                        Outputs = jsPart.Outputs ?? element.Outputs,
                        Inputs = element.Inputs,
                        Type = element.Type,
                        Name = jsPart.Name ?? string.Empty,
                        Uid = jsPart.Uid,
                        Icon = element.Icon,
                        Model = jsPart.Model,
                        OutputConnections = jsPart.Connections?.Select(x => new FlowConnection
                        {
                            Input = x.Input,
                            Output = x.Output,
                            InputNode = x.Node
                        }).ToList() ?? new List<FlowConnection>()
                    });
                    y += 150;
                }

                if (invalid)
                    return (false, null, null);

                var flow = new Flow()
                {
                    Template = jst.Name,
                    Name = jst.Name,
                    Author = jst.Author,
                    Type = jst.Type,
                    Uid = Guid.Empty,
                    Description = jst.Description,
                    Parts = flowParts,
                    Enabled = true
                };

                return (true, jst, flow);
            }
            catch (Exception ex)
            {
                Logger.Instance.ELog("Template: " + jst.Name);
                Logger.Instance.ELog("Error reading template: " + ex.Message + Environment.NewLine +
                                     ex.StackTrace);
            }

        }
        catch (Exception ex)
        {
            Logger.Instance.ELog("Error reading template: " + ex.Message + Environment.NewLine + ex.StackTrace);
        }
        return (false, null, null);
    }
    
    
    private FlowTemplate LoadModernTemplate(string json)
    {
        var flow = JsonSerializer.Deserialize<Flow>(json);
        var template = new FlowTemplate();
        template.Name = flow.Name;
        template.Description = flow.Description;
        template.Author = flow.Author;
        template.Tags = flow.Properties.Tags;

        template.SkipTreeShaking = true;
        template.Save = true; // this means the flow will be saved automatically and not opened when creating a flow based on this template
        template.Parts = new();
        foreach(var fp in flow.Parts)
        {
            var tfp = new FlowTemplatePart();
            tfp.Uid = fp.Uid;
            tfp.Model = fp.Model;
            tfp.Name = fp.Name;
            tfp.Outputs = fp.Outputs;
            tfp.xPos = (int)fp.xPos;
            tfp.yPos = (int)fp.yPos;
            tfp.Node = fp.FlowElementUid;
            tfp.Connections = fp.OutputConnections?.Select(x => new FlowTemplateConnection()
            {
                Node = x.InputNode,
                Input = x.Input,
                Output = x.Output
            })?.ToList() ?? new();
            template.Parts.Add(tfp);
        }

        template.Fields = new();
        foreach (var field in flow.Properties?.Fields ?? new())
        {
            var tf = new TemplateField();
            tf.Name = field.Name;
            tf.Label = field.Name.Replace("_" , " ");
            tf.Default = field.DefaultValue;
            tf.Help = field.Description;
            if (string.IsNullOrWhiteSpace(field.FlowElementField) == false && Regex.IsMatch(field.FlowElementField,
                    @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\.[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                // this is a strong name to a field
                var parts = field.FlowElementField.Split('.');
                tf.Uid = Guid.Parse(parts[0]);
                tf.Name = parts[1];
            }
            tf.Type = field.Type switch
            {
                FlowFieldType.Directory => "Directory",
                FlowFieldType.Boolean => "Switch",
                FlowFieldType.Number => "Int",
                _ => "Text"
            };

            if (field.Type == FlowFieldType.Directory && string.IsNullOrWhiteSpace(tf.Default as string))
                tf.Default = DirectoryHelper.GetUsersHomeDirectory();
            
            template.Fields.Add(tf);

            if (string.IsNullOrWhiteSpace(field.IfName))
                continue;
            var other = flow.Properties.Fields.FirstOrDefault(x => x.Name == field.IfName);
            if (other == null)
                continue;

            var condition = new Condition();
            condition.Property = other.Name;
            if (other.Type == FlowFieldType.Boolean)
                condition.Value = field.IfValue?.ToLowerInvariant()?.Trim() == "true";
            else if (other.Type == FlowFieldType.Number && int.TryParse(field.IfValue?.Trim(), out int iOther))
                condition.Value = iOther;
            else
                condition.Value = field.IfValue;
            condition.IsNot = field.IfNot;
            tf.Conditions ??= new();
            tf.Conditions.Add(condition);
        }

        return template;
    }
}