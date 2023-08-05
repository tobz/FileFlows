﻿namespace FileFlowTests.Tests;

/// <summary>
/// Tests for variable replacements
/// </summary>
[TestClass]
public class VariablesTest
{
    /// <summary>
    /// Tests plex variables in folder names are not excaped
    /// </summary>
    [TestMethod]
    public void PlexTests()
    {
        var variables = new Dictionary<string, object>();
        Assert.AreEqual("ShowName (2020) {tmdb-123456}", VariablesHelper.ReplaceVariables("ShowName (2020) {tmdb-123456}", variables, stripMissing: true));
        Assert.AreEqual("ShowName (2020) {tvdb-123456}", VariablesHelper.ReplaceVariables("ShowName (2020) {tvdb-123456}", variables, stripMissing: true));
        Assert.AreEqual("ShowName (2020) {imdb-123456}", VariablesHelper.ReplaceVariables("ShowName (2020) {imdb-123456}", variables, stripMissing: true));
        
        Assert.AreEqual("ShowName (2020) {tmdb-123456} missing", VariablesHelper.ReplaceVariables("ShowName (2020) {tmdb-123456} {missing}", variables, stripMissing: false));
        Assert.AreEqual("ShowName (2020) {tvdb-123456} missing", VariablesHelper.ReplaceVariables("ShowName (2020) {tvdb-123456} {missing}", variables, stripMissing: false));
        Assert.AreEqual("ShowName (2020) {imdb-123456} missing", VariablesHelper.ReplaceVariables("ShowName (2020) {imdb-123456} {missing}", variables, stripMissing: false));
        variables.Add("tmdb-123456", "bobby");
        variables.Add("tvdb-123456", "drake");
        variables.Add("imdb-123456", "iceman");
        Assert.AreEqual("ShowName (2020) bobby", VariablesHelper.ReplaceVariables("ShowName (2020) {tmdb-123456}", variables, stripMissing: true));
        Assert.AreEqual("ShowName (2020) drake", VariablesHelper.ReplaceVariables("ShowName (2020) {tvdb-123456}", variables, stripMissing: true));
        Assert.AreEqual("ShowName (2020) iceman", VariablesHelper.ReplaceVariables("ShowName (2020) {imdb-123456}", variables, stripMissing: true));
    }
    
}