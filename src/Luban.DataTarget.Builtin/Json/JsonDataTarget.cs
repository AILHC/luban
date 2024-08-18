using System.Text.Json;
using Luban.DataTarget;
using Luban.Defs;
using Luban.Utils;

namespace Luban.DataExporter.Builtin.Json;

[DataTarget("json")]
public class JsonDataTarget : DataTargetBase
{
    protected override string DefaultOutputFileExt => "json";

    public static bool UseCompactJson => EnvManager.Current.GetBoolOptionOrDefault("json", "compact", true, false);
    public override AggregationType AggregationType
    {
        get
        {
            var monolithic = EnvManager.Current.GetBoolOptionOrDefault("json", "monolithic", true, false);
            return monolithic ? AggregationType.Tables : AggregationType.Table;
        }
    }

    protected virtual JsonDataVisitor ImplJsonDataVisitor => JsonDataVisitor.Ins;
    protected void WriteAsArray(List<Record> records, Utf8JsonWriter x, JsonDataVisitor jsonDataVisitor)
    {
        x.WriteStartArray();
        foreach (var d in records)
        {
            d.Data.Apply(jsonDataVisitor, x);
        }
        x.WriteEndArray();
    }
    public virtual void WriteTable(DefTable table, List<Record> records, Utf8JsonWriter x)
    {
        this.WriteAsArray(records, x, ImplJsonDataVisitor);
    }

    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {
        var ss = new MemoryStream();
        var jsonWriter = new Utf8JsonWriter(ss, new JsonWriterOptions()
        {
            Indented = !UseCompactJson,
            SkipValidation = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        WriteTable(table, records, jsonWriter);
        jsonWriter.Flush();
        return new OutputFile()
        {
            File = $"{table.OutputDataFile}.{OutputFileExt}",
            Content = DataUtil.StreamToBytes(ss),
        };
    }
    public override OutputFile ExportTables(List<DefTable> tables)
    {
        // var path = EnvManager.Current.GetOptionOrDefault("json", "outputFile", true,$"all.{OutputFileExt}");
        var ss = new MemoryStream();
        var jsonWriter = new Utf8JsonWriter(ss, new JsonWriterOptions()
        {
            Indented = !UseCompactJson,
            SkipValidation = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        var ctx = GenerationContext.Current;
        jsonWriter.WriteStartObject();
        foreach (var table in tables)
        {

            var tableName = table.OutputDataFile;
            var records = ctx.GetTableExportDataList(table);
            jsonWriter.WritePropertyName(tableName);
            WriteTable(table, records, jsonWriter);

        }
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
        var fileName = $"all.{OutputFileExt}";
        return new OutputFile()
        {
            File = fileName,
            Content = DataUtil.StreamToBytes(ss),
        };
    }

}
