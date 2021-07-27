using System;
using System.IO;
using System.Linq;
using Goblinfactory.PrettyJson;
using CommandLine;
using DevLab.JmesPath;
using Newtonsoft.Json.Linq;

namespace Jmespath.Tool
{
  [Verb("qry", HelpText = "query a json (.js) or javascript (.json) source using JmesPath queries.")]
  public class Qry
  {
    [Value(0, HelpText = "Relative path to *.js or *.json file.", Required = true)]
    public string Path { get; set; }

    [Value(1, HelpText = "JmesPath query text expression e.g. 'lists[].{ listId:id, name:name, closed:closed, pos:pos}'", Required = true)]
    public string Expression { get; set; }

    [Value(2, HelpText = "output file", Required = false)]
    public string Output { get; set; }

    [Value(3, HelpText = "Verbose", Required = false)]
    public bool Verbose { get; set; } = false;
  }


  class Program
  {
    private static string GetJavscriptJson(Qry qry, string raw)
    {
      // strip out everything up until the first {
      // the assumption is that a javscript file starts with export default XYZ = { ... json goes here }
      if (string.IsNullOrEmpty(raw) || !raw.Contains('{')) throw new ArgumentException($"Could not find any json in {qry.Path}.");
      return raw.Substring(raw.IndexOf('{'));
    }

    static void Main(string[] args)
    {
      bool verbose = false;
      if (args.Contains("-v"))
      {
        args = args.Where(a => !a.Contains("-v")).ToArray();
        verbose = true;
      }
      CommandLine.Parser.Default.ParseArguments<Qry>(args).WithParsed<Qry>(query => RunQry(query, verbose));
    }

    static void RunQry(Qry query, bool verbose)
    {
      try
      {
        _runQry(query);
      }
      catch (Exception ex)
      {
        Error(ex.Message, "");
        if (verbose) Console.WriteLine(ex.ToString());
      }
    }

    static void _runQry(Qry query)
    {
      var fi = new FileInfo(query.Path);
      if (!fi.Exists) throw new ArgumentException("Could not find file", query.Path);

      var input = File.ReadAllText(query.Path);

      // if file is .js
      var json = fi.Extension switch
      {
        ".js" => GetJavscriptJson(query, input),
        ".json" => input,
        _ => throw new ArgumentException($"Extension type {fi.Extension} is not supported", "Options.Path")
      };

      var jms = new JmesPath();
      var result = jms.Transform(json, query.Expression);
      var jsonResult = IsList(result) ? $"{{ \"results\":{result} }}" : result;
      var printer = new PrettyPrinter(PrettyConfig.CreateDefault());
      printer.Print(jsonResult);

      Console.WriteLine("");

    }

    // return true if the json fragment is a raw list e.g. produced from list[].
    static bool IsList(string json)
    {
      return json.Trim().StartsWith("[");
    }

    static void Error(string title, string text)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write(title);
      Console.ResetColor();
      Console.WriteLine($" {text}");
    }

  }

  // style ... .js or .json
  // if .js then assumes the first line starts with export const XXX = {  and replaces first line with {
  // if first line not that...then reply with error message

}