using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Recovery tool: decodes phone-only ASTC textures exported from the APK so they can be rewritten as RGBA assets.
public static class RecoveryDecodeAstc
{
	public static void Run()
	{
		string outDir = "E:/eyes/logs/astc";
		Directory.CreateDirectory(outDir);
		StringBuilder report = new StringBuilder();
		foreach (string path in Directory.GetFiles("Assets", "*.texture2D", SearchOption.AllDirectories))
		{
			string text = File.ReadAllText(path);
			int format = ReadInt(text, "m_TextureFormat: ");
			if (format != 48)
			{
				continue;
			}
			int width = ReadInt(text, "m_Width: ");
			int height = ReadInt(text, "m_Height: ");
			int mips = ReadInt(text, "m_MipCount: ");
			int hexStart = text.IndexOf("_typelessdata: ", StringComparison.Ordinal) + 15;
			int hexEnd = text.IndexOf('\n', hexStart);
			string hex = text.Substring(hexStart, hexEnd - hexStart).Trim();
			byte[] raw = new byte[hex.Length / 2];
			for (int i = 0; i < raw.Length; i++)
			{
				raw[i] = (byte)((Hex(hex[i * 2]) << 4) | Hex(hex[i * 2 + 1]));
			}
			Texture2D texture = new Texture2D(width, height, TextureFormat.ASTC_4x4, mips, true);
			texture.LoadRawTextureData(raw);
			texture.Apply(false, false);
			string outPath = Path.Combine(outDir, path.Replace('\\', '_').Replace('/', '_') + ".rgba");
			double sum = 0.0;
			using (FileStream stream = File.Create(outPath))
			{
				for (int mip = 0; mip < mips; mip++)
				{
					Color32[] pixels = texture.GetPixels32(mip);
					byte[] bytes = new byte[pixels.Length * 4];
					for (int p = 0; p < pixels.Length; p++)
					{
						bytes[p * 4] = pixels[p].r;
						bytes[p * 4 + 1] = pixels[p].g;
						bytes[p * 4 + 2] = pixels[p].b;
						bytes[p * 4 + 3] = pixels[p].a;
						if (mip == 0)
						{
							sum += pixels[p].r + pixels[p].g + pixels[p].b;
						}
					}
					stream.Write(bytes, 0, bytes.Length);
				}
			}
			report.AppendLine(path + " " + width + "x" + height + " mips=" + mips + " avg=" + (sum / (3.0 * width * height * 255.0)).ToString("F4") + " -> " + outPath);
		}
		File.WriteAllText(Path.Combine(outDir, "report.txt"), report.ToString());
		EditorApplication.Exit(0);
	}

	private static int ReadInt(string text, string key)
	{
		int start = text.IndexOf(key, StringComparison.Ordinal) + key.Length;
		int end = start;
		while (char.IsDigit(text[end]))
		{
			end++;
		}
		return int.Parse(text.Substring(start, end - start));
	}

	private static int Hex(char c)
	{
		return c <= '9' ? c - '0' : (c - 'a' + 10);
	}
}
