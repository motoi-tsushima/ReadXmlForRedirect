using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ReadXmlForRedirect
{
    internal class ReadXmlElements
    {
        /// <summary>
        /// BloggerのエクスポートXmlを読み込む
        /// </summary>
        /// <param name="xmlFilePath">BloggerのエクスポートXmlファイルのパス</param>
        /// <returns>Trueならエラー</returns>
        public bool ReadBloggerXml(string xmlFilePath)
        {
            XNamespace nsAtom = "http://www.w3.org/2005/Atom";

            XElement root = XElement.Load(xmlFilePath);

            var entrys = root.Elements(nsAtom + "entry");

            List<List<string>> lists = new List<List<string>>();

            foreach (var entry in entrys)
            {
                var elements = entry.Elements();

                foreach (var element in elements)
                {
                    List<string> list = new List<string>();

                    if (element.Name == nsAtom + "link")
                    {
                        bool alternate = false;

                        foreach (var attr in element.Attributes())
                        {
                            if (attr.Name == "rel" && attr.Value == "alternate")
                            {
                                alternate = true;
                            }
                            if (attr.Name == "href" && alternate)
                            {
                                list.Add(attr.Value);
                            }
                            if (attr.Name == "title" && alternate)
                            {
                                list.Add(attr.Value);
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        lists.Add(list);
                    }
                }
            }

            foreach (List<string> vals in lists)
            {
                if (vals.Count < 2) continue;

                //--- タイトル,URL の順に出力する -----
                string output = vals[1] + "," + vals[0];
                Console.WriteLine(output);
            }

            return false;
        }

        /// <summary>
        /// Wordpress のエクスポートXmlを読み込む
        /// </summary>
        /// <param name="xmlFilePath">Wordpress のエクスポートXmlファイルのパス</param>
        /// <returns>Trueならエラー</returns>
        public bool ReadWordpressXml(string xmlFilePath)
        {
            XElement root = XElement.Load(xmlFilePath);

            var items = root.Elements("channel").Elements("item");

            List<List<string>> lists = new List<List<string>>();

            foreach (var item in items)
            {
                var elements = item.Elements();

                List<string> list = new List<string>();
                foreach (var element in elements)
                {
                    if (element.Name == "title" || element.Name == "link")
                    {
                        list.Add(element.Value);
                    }
                }
                if(list.Count > 0)
                {
                    lists.Add(list);
                }
            }

            foreach (List<string> vals in lists)
            {
                if (vals.Count < 2) continue;

                //--- タイトル,URL の順に出力する -----
                string output = vals[0] + "," + vals[1];
                Console.WriteLine(output);
            }

            return false;
        }


        public enum RedirectFileMode
        {
            non,
            CSV,
            htaccess1,
            htaccess2
        }

        public bool MakeRedirectFile(string InputFilePath, string OutputFilePath, RedirectFileMode redirectFileMode)
        {
            List<List<string>> inputTable = new List<List<string>>();

            using(var ifs = new StreamReader(InputFilePath, Encoding.UTF8))
            {
                while (!ifs.EndOfStream)
                {
                    var inputLine = ifs.ReadLine();
                    if (inputLine == null) continue;

                    string[] inputColumns = inputLine.Split(',');
                    if (inputColumns.Length < 2) continue;

                    List<string> row = new List<string>();
                    row.Add(inputColumns[0]);
                    row.Add(inputColumns[1]);
                    inputTable.Add(row);
                }

            }

            List<List<string>> outputTable = new List<List<string>>();

            using (var ofs = new StreamReader(OutputFilePath, Encoding.UTF8))
            {
                while (!ofs.EndOfStream)
                {
                    var outputLine = ofs.ReadLine();
                    if (outputLine == null) continue;

                    string[] outputColumns = outputLine.Split(',');
                    if (outputColumns.Length < 2) continue;

                    List<string> row = new List<string> ();
                    row.Add(outputColumns[0]);
                    row.Add(outputColumns[1]);
                    outputTable.Add(row);
                }
            }

            //互いのColumns[0]をキーにして、キーの一致する、双方のColumns[1]の値をCSVへ出力する。-----

            List<List<string>> resultTable = new List<List<string>>();

            foreach(var inputRow in inputTable)
            {
                bool found = false;
                var searchWord = inputRow[0];
                string inputURL = inputRow[1];
                string outputURL = string.Empty;

                foreach(var outputRow in outputTable)
                {
                    if (searchWord == null) break;

                    if (outputRow[0] == null) continue;

                    if(searchWord == outputRow[0])
                    {
                        found = true; 
                        outputURL = outputRow[1];
                        break;
                    }
                }

                if (found)
                {
                    List<string> resultRow = new List<string>();
                    resultRow.Add(inputURL); 
                    resultRow.Add(outputURL);
                    resultTable.Add(resultRow);
                    found = false;
                }
            }

            if(redirectFileMode == RedirectFileMode.CSV)
            {
                foreach (var resultRow in resultTable)
                {
                    Console.WriteLine("{0},{1}", resultRow[0], resultRow[1]);
                }
            }
            else if (redirectFileMode == RedirectFileMode.htaccess1)
            {
                foreach (var resultRow in resultTable)
                {
                    string[] inputDomainWords = resultRow[0].Split('/');
                    string inputLocal = string.Empty;
                    for (int i = 0; i < inputDomainWords.Length; i++)
                    {
                        if (i < 3) continue;
                        if(i == 3)
                        {
                            inputLocal += ("^" + inputDomainWords[i]);
                        }
                        else
                        {
                            inputLocal += ("/" + inputDomainWords[i]);
                        }
                    }
                    inputLocal += "$";

                    string[] outputDomainWords = resultRow[1].Split('/');
                    string outputLocal = string.Empty;
                    for (int i = 0; i < outputDomainWords.Length; i++)
                    {
                        if (i < 3) continue;
                        if (i == 3)
                        {
                            outputLocal += ("^" + outputDomainWords[i]);
                        }
                        else
                        {
                            outputLocal += ("/" + outputDomainWords[i]);
                        }
                    }

                    Console.WriteLine("RewriteRule {0} {1} [R=301,L]", inputLocal, outputLocal);
                }
            }
            else if (redirectFileMode == RedirectFileMode.htaccess2)
            {
                foreach (var resultRow in resultTable)
                {
                    string[] inputDomainWords = resultRow[0].Split('/');
                    string inputLocal = string.Empty;
                    for (int i = 0; i < inputDomainWords.Length; i++)
                    {
                        if (i < 3) continue;
                        inputLocal += ("/" + inputDomainWords[i]);
                    }

                    Console.WriteLine("Redirect permanent {0} {1}", inputLocal, resultRow[1]);
                }
            }
            else
            {
                //あり得ないが、例外はCSVで出力してしまう。
                foreach (var resultRow in resultTable)
                {
                    Console.WriteLine("{0},{1}", resultRow[0], resultRow[1]);
                }
            }

            return false;
        }
    }
}
