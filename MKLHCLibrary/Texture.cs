using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MKLHCLibrary
{
    public class Texture
    {
        public string vtable { get; set; }
        public string version { get; set; } = "vjson-1";
        public string[] imports { get; set; }
        public string[] exports { get; set; }
        public Segment[] segments { get; set; }

        public string ToJson() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class Segment
    {
        public string debug_type_name { get; set; }
        public string name { get; set; }
        public int size { get; set; }

        public TextureObject[] exported_objects { get; set; }
        public CommonObject[] common_objects { get; set; }
    }

    [JsonObject()]
    public class Object
    {
        [JsonProperty("$debug_type_name")]
        public string debug_type_name { get; set; }

        [JsonProperty("$guid")]
        public string guid { get; set; }

        [JsonProperty("$name")]
        public string name { get; set; }

        public string vtable { get; set; }
    }

    [JsonObject()]
    public class TextureObject : Object
    {
        public string path { get; set; }
        public TextureData data { get; set; }
        public string[] format { get; set; }
        public string type { get; set; }
        public List<uint[]> mip_offsets { get; set; }
        public long[] layout { get; set; }
        public uint[] width { get; set; }
        public uint[] height { get; set; }
        public uint[] depth { get; set; }
        public uint[] mip_levels { get; set; }
        public uint[] array_layers { get; set; }
        public uint[] scale_multipliers { get; set; }
        public bool is_packed { get; set; }
    }

    [JsonObject()]
    public class CommonObject : Object
    {
        public float quality_level { get; set; }
        public bool generate_mips { get; set; }
        public Buffer buffer { get; set; }
    }

    [JsonObject()]
    public class TextureData
    {
        [JsonProperty("$debug_type_name")]
        public string debug_type_name { get; set; }
        public string nvn { get; set; }
    }

    [JsonObject()]
    public class Buffer
    {
        public int size { get; set; }
        public int align_log2 { get; set; }
        public byte[] data { get; set; }
    }
}
