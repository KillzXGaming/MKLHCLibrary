using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using MKLHCLibrary;
using Newtonsoft.Json;
using Toolbox.Core;
using Toolbox.Core.Imaging;

namespace MKLHC.TextureExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"Converter by KillXGaming");
                Console.WriteLine($"Usage: MKLHC.TextureExporter.exe (texture file.import) (Optional Arguments)");
                Console.WriteLine($"Optional Arguments: -dds (Uses .dds instead of .png)");
                return;
            }

            bool useDDS = args.Contains("-dds");

            PluginManager.LoadPlugins();
            foreach (var arg in args)
            {
                if (Directory.Exists(arg))
                {
                    foreach (var f in Directory.GetFiles(arg))
                        OpenFile(f, useDDS);
                }
                else
                    OpenFile(arg, useDDS);
            }
        }

        static void OpenFile(string filePath, bool useDDS)
        {
            if (!File.Exists(filePath))
                return;

            if (filePath.EndsWith(".import"))
                ConvertTexture(filePath, useDDS);
        }

        static void ConvertTexture(string filePath, bool useDDS)
        {
            string json = File.ReadAllText(filePath);
            var texture = JsonConvert.DeserializeObject<Texture>(json);

            var buffer = new byte[0];

            List<STGenericTexture> textures = new List<STGenericTexture>();
            foreach (var segment in texture.segments)
            {
                if (segment.name == "gpu-texture-nvn")
                {
                    buffer = segment.common_objects[0].buffer.data;
                }
                else if (segment.name == "default")
                {
                    var textureObject = segment.exported_objects[0];
                    for (int i = 0; i < textureObject.format.Length; i++)
                    {
                        var format = textureObject.format[i];
                        if (!FormatList.ContainsKey(format))
                        {
                            Console.WriteLine($"Unsupported format! {format}");
                            continue;
                        }

                        var genericFormat = FormatList[format];
                        var layout = textureObject.layout[i];

                        int layout1 = (int)(layout & uint.MaxValue);
                        int layout2 = (int)(layout >> 32);

                        GenericTexture genericTexture = new GenericTexture();
                        genericTexture.Name = textureObject.name;
                        genericTexture.Width = textureObject.width[i];
                        genericTexture.Height = textureObject.height[i];
                        genericTexture.Depth = textureObject.depth[i];
                        genericTexture.ArrayCount = textureObject.array_layers[i];
                        genericTexture.MipCount = textureObject.mip_levels[i];
                        var mipOffsets = textureObject.mip_offsets[i];
                        genericTexture.Platform = new SwitchSwizzle(genericFormat)
                        {
                            MipOffsets = mipOffsets,
                            BlockHeightLog2 = (uint)(layout1 & 7),
                        };
                        textures.Add(genericTexture);
                        break;
                    };
                }
            }

            string folder = Path.GetDirectoryName(filePath);
            foreach (GenericTexture tex in textures)
            {
                 tex.ImageData = buffer;
                if (useDDS)
                    tex.Export($"{folder}\\{tex.Name}_{tex.Platform.OutputFormat}.dds", new TextureExportSettings());
                else
                    tex.Export($"{folder}\\{tex.Name}_{tex.Platform.OutputFormat}.png", new TextureExportSettings());
                 break;
            }
        }

        static Dictionary<string, TexFormat> FormatList = new Dictionary<string, TexFormat>()
        {
            { "k_vgpu_format_bc1_rgb_unorm_block", TexFormat.BC1_UNORM },
            { "k_vgpu_format_bc1_rgb_srgb_block", TexFormat.BC1_SRGB },
            { "k_vgpu_format_bc2_rgb_unorm_block", TexFormat.BC2_UNORM },
            { "k_vgpu_format_bc2_rgb_srgb_block", TexFormat.BC2_SRGB },
            { "k_vgpu_format_bc3_rgb_unorm_block", TexFormat.BC3_UNORM },
            { "k_vgpu_format_bc3_rgb_srgb_block", TexFormat.BC3_SRGB },
            { "k_vgpu_format_bc4_unorm_block", TexFormat.BC4_UNORM },
            { "k_vgpu_format_bc4_snorm_block", TexFormat.BC4_SNORM },
            { "k_vgpu_format_bc5_unorm_block", TexFormat.BC5_UNORM },
            { "k_vgpu_format_bc5_snorm_block", TexFormat.BC5_SNORM },

            { "k_vgpu_format_bc7_unorm_block", TexFormat.BC7_UNORM },
            { "k_vgpu_format_bc7_srgb_block", TexFormat.BC7_SRGB },
            { "k_vgpu_format_astc_4x4_unorm_block", TexFormat.ASTC_4x4_UNORM },
            { "k_vgpu_format_astc_4x4_srgb_block", TexFormat.ASTC_4x4_SRGB },
        };

        public class GenericTexture : STGenericTexture
        {
            public byte[] ImageData;

            public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0)
            {
                return ImageData;
            }

            public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
