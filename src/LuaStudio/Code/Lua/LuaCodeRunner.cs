using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaStudio.Code.Lua
{
    class LuaCodeRunner : ICodeRunner
    {
        /// <summary>
        /// Run
        /// </summary>
        public int Run(TextReader sourceReader, string sourceFile)
        {
            String fileName = sourceFile;
            if (!File.Exists(fileName))
            {
                if (sourceReader == null) throw new ArgumentException("No code source provided.");
                fileName = Path.GetTempFileName() + ".lua";
                using(var rdr=new StreamWriter(fileName))
                {
                    String line = null;
                    while ((line = sourceReader.ReadLine()) != null)
                        rdr.WriteLine(line);
                }
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(Path.GetDirectoryName(typeof(AppContext).Assembly.Location), "lua.exe");
            psi.Arguments = String.Format("\"{0}\"", fileName);
            psi.UseShellExecute = true;
            psi.WorkingDirectory = Path.GetDirectoryName(fileName);
            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }

    }
}
