namespace FileFlow.Server.Helpers
{
    using FileFlow.Shared.Models;
    using FileFlow.Plugin;
    using FileFlow.Server.Workers;

    public class FlowExecutor
    {
        public Flow Flow { get; set; }

        public ILogger Logger { get; set; }

        public delegate void PartPercentageUpdate(float percentage);
        public event PartPercentageUpdate OnPartPercentageUpdate;

        public delegate void StepChange(int currentStep, string stepName);

        public event StepChange OnStepChange;

        public Node CurrentNode;

        public Task<NodeParameters> Run(string input)
        {
            return Task.Run(() =>
            {
                var args = new NodeParameters();
                args.FileName = input;
                args.WorkingFile = input;

                args.PartPercentageUpdate = (float percentage) => OnPartPercentageUpdate?.Invoke(percentage);

                var fiInput = new System.IO.FileInfo(input);
                args.OutputFile = @"D:\videos\processed\" + fiInput.Name.Replace(fiInput.Extension, ".mkv");
                args.Result = NodeResult.Success;
                args.Logger = Logger ?? new FlowLogger();
                args.GetToolPath = (string name) => new Controllers.ToolController().GetByName(name)?.Path ?? "";
                bool flowCompleted = false;
                int count = 0;

                // find the first node
                var part = Flow.Parts.Where(x => x.Inputs == 0).FirstOrDefault();
                if (part == null)
                    return args; // no node to execute

                int step = 0;
                if (OnStepChange != null)
                    OnStepChange(step, part.Name);

                while (flowCompleted == false && count < 20)
                {
                    try
                    {

                        args.Logger.DLog("Executing part:" + part.Name);
                        CurrentNode = NodeHelper.LoadNode(part);

                        ++step;
                        if (OnStepChange != null)
                            OnStepChange(step, CurrentNode.Name);

                        args.Logger.DLog("node: " + CurrentNode);
                        int output = CurrentNode.Execute(args);
                        if (CurrentNode == null)
                        {
                            // happens when canceled    
                            args.Logger.ELog("node was canceled error code:", CurrentNode);
                            args.Result = NodeResult.Failure;
                            flowCompleted = true;
                            break;
                        }
                        args.Logger.DLog("output: " + output);
                        if (output == -1)
                        {
                            // the execution failed                     
                            args.Logger.ELog("node returned error code:", CurrentNode);
                            args.Result = NodeResult.Failure;
                            flowCompleted = true;
                            break;
                        }
                        var outputNode = part.OutputConnections?.Where(x => x.Output == output)?.FirstOrDefault();
                        if (outputNode == null)
                        {
                            args.Logger.DLog("flow completed");
                            // flow has completed
                            flowCompleted = true;
                            break;
                        }
                        // we need the connection details
                        // need recurision here, since a node could have multiple output connections from the same output
                        // and we need to clone the input parameters at this point, since we may alter them differently on each path
                        // for now we're just doing one
                        part = outputNode == null ? null : Flow.Parts.Where(x => x.Uid == outputNode.InputNode).FirstOrDefault();
                        if (part == null)
                        {
                            // couldnt find the connection, maybe bad data, but flow has now finished
                            args.Logger.WLog("couldnt find output node, flow completed: " + outputNode?.Output);
                            flowCompleted = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        args.Result = NodeResult.Failure;
                        args.Logger.ELog("Execution error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                }

                return args;
            });
        }

        public async Task Cancel()
        {
            if (CurrentNode != null)
            {
                var cn = CurrentNode;
                CurrentNode = null;
                await cn.Cancel();
            }
        }
    }
}