using Luban.Job.Cfg.Datas;
using System.Collections.Generic;

namespace Luban.Job.Cfg.DataSources
{
    public class Record
    {
        public DBean Data { get; }

        public string Source { get; }

        public string SheetName { get; }

        public List<string> Tags { get; }

        public bool IsNotFiltered(List<string> excludeTags)
        {
            if (Tags == null)
            {
                return true;
            }
            return Tags.TrueForAll(t => !excludeTags.Contains(t));
        }

        public Record(DBean data, string source, List<string> tags,string sheetName=null)
        {
            Data = data;
            Source = source;
            SheetName = sheetName;
            Tags = tags;
        }
    }
}
