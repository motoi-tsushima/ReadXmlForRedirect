using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ReadXmlForRedirect
{
    internal class ReadXmlElements
    {
        /// <summary>
        /// BloggerのエクスポートXmlを読み込む
        /// </summary>
        /// <param name="xmlFilePath">BloggerのエクスポートXmlファイルのパス</param>
        /// <returns>Trueならエラー</returns>
        /// <remarks>結果は標準出力へ出力する</remarks>
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

                //出力用タイトルとURLを生成する。
                string blogTitle = vals[1].Replace(",", "，"); //カンマは全角に変換する。
                string blogUrl = vals[0];

                //--- タイトル,URL の順に出力する -----
                string output = blogTitle + "," + blogUrl;
                Console.WriteLine(output);
            }

            return false;
        }

        /// <summary>
        /// Wordpress のエクスポートXmlを読み込む
        /// </summary>
        /// <param name="xmlFilePath">Wordpress のエクスポートXmlファイルのパス</param>
        /// <returns>Trueならエラー</returns>
        /// <remarks>結果は標準出力へ出力する</remarks>
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

                //出力用タイトルとURLを生成する。
                string blogTitle = vals[0].Replace(",", "，"); //カンマは全角に変換する。
                string blogUrl = vals[1];

                //--- タイトル,URL の順に出力する -----
                string output = blogTitle + "," + blogUrl;
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

        /// <summary>
        /// リダイレクト設定を出力する
        /// </summary>
        /// <param name="InputFilePath">ソースのエクスポートXmlのファイル名パス</param>
        /// <param name="OutputFilePath">リダイレクト後エクスポートXmlのファイル名パス</param>
        /// <param name="redirectFileMode">レダイレクト設定の出力形式を指定する</param>
        /// <returns>Trueならエラー</returns>
        /// <remarks>結果は標準出力へ出力する</remarks>
        public bool MakeRedirectFile(string InputFilePath, string OutputFilePath, RedirectFileMode redirectFileMode)
        {
            //---- ソースのタイトルとURLのコレクション --------
            List<List<string>> inputTable = new List<List<string>>();

            //==== ソースのエクスポートXmlのファイルを読み込む ====================
            using (var ifs = new StreamReader(InputFilePath, Encoding.UTF8))
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

            //---- リダイレクト後のタイトルとURLのコレクション --------
            List<List<string>> outputTable = new List<List<string>>();

            //==== リダイレクト後のエクスポートXmlのファイルを読み込む ====================
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

            //----- 結果出力用(リダイレクト設定),ソースURLとリダイレクト後URL --------------------
            List<List<string>> resultTable = new List<List<string>>();

            //================================================================================================
            //互いのColumns[0](タイトル)をキーにして、
            //キーの一致する双方のColumns[1]の値をCSV1行へ出力する。
            //========
            foreach (var inputRow in inputTable)
            {
                //----- ソース行単位でループする ----------
                bool found = false;
                var searchWord = inputRow[0];
                string inputURL = inputRow[1];  //ソースURL
                string outputURL = string.Empty;    //リダイレクト後URL

                //----- リダイレクト後リストの全件のタイトルを検索する　---------
                foreach (var outputRow in outputTable)
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
                    //----- 見つかったら、ソースURLとリダイレクト後URLを、1行に出力する -------
                    List<string> resultRow = new List<string>();
                    resultRow.Add(inputURL); 
                    resultRow.Add(outputURL);
                    resultTable.Add(resultRow);
                    found = false;
                }
            }

            //**** 単純CSV出力モード *********
            if (redirectFileMode == RedirectFileMode.CSV)
            {
                foreach (var resultRow in resultTable)
                {
                    Console.WriteLine("{0},{1}", resultRow[0], resultRow[1]);
                }
            }
            //**** htaccess RewriteRule設定 出力モード *********
            else if (redirectFileMode == RedirectFileMode.htaccess1)
            {
                foreach (var resultRow in resultTable)
                {
                    //==== 取得URL0からRewriteRuleの第1パラメータ(元URL)を生成する ======
                    //string[] inputDomainWords = resultRow[0].Split('/');
                    //string inputLocal = string.Empty;
                    //for (int i = 0; i < inputDomainWords.Length; i++)
                    //{
                    //    if (i < 3) continue;
                    //    if(i == 3)
                    //    {
                    //        inputLocal += ("^" + inputDomainWords[i]);
                    //    }
                    //    else
                    //    {
                    //        inputLocal += ("/" + inputDomainWords[i]);
                    //    }
                    //}
                    //inputLocal += "$";
                    string[] inputLocals = GetDomainFoldername(resultRow[0]);

                    //==== 取得URL1からRewriteRuleの第2パラメータ(置換後URL)を生成する ======
                    //string[] outputDomainWords = resultRow[1].Split('/');
                    //string outputLocal = string.Empty;
                    //for (int i = 0; i < outputDomainWords.Length; i++)
                    //{
                    //    if (i < 3) continue;
                    //    if (i == 3)
                    //    {
                    //        outputLocal += ("^" + outputDomainWords[i]);
                    //    }
                    //    else
                    //    {
                    //        outputLocal += ("/" + outputDomainWords[i]);
                    //    }
                    //}
                    string[] outputLocals = GetDomainFoldername(resultRow[1]);

                    //==== RewriteRuleの設定値を出力する ==========
                    Console.WriteLine("RewriteRule ^{0}$ {1} [R=302,L]", inputLocals[1], outputLocals[1]);
                }
            }
            //**** htaccess Redirect設定 出力モード *********
            else if (redirectFileMode == RedirectFileMode.htaccess2)
            {
                foreach (var resultRow in resultTable)
                {
                    //===== Redirect permanent の第1パラメータ(元URL)を生成する ========
                    //string[] inputDomainWords = resultRow[0].Split('/');
                    //string inputLocal = string.Empty;
                    //for (int i = 0; i < inputDomainWords.Length; i++)
                    //{
                    //    if (i < 3) continue;
                    //    inputLocal += ("/" + inputDomainWords[i]);
                    //}
                    string[] inputDomainWords = GetDomainFoldername(resultRow[0]);

                    //==== Redirect permanent の設定値を出力する ==========
                    Console.WriteLine("Redirect permanent {0} {1}", inputDomainWords[1], resultRow[1]);
                }
            }
            //**** あり得ない *********
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

        /// <summary>
        /// URLをドメイン名とフォルダー名に分割する
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>[0]ドメイン名 , [1]フォルダー名</returns>
        private string[] GetDomainFoldername(string url)
        {
            string[] result = new string[2];
            string domainName = string.Empty;
            string folderName = string.Empty;

            string[] domainWords = url.Split('/');
            string folderNames = string.Empty;
            for (int i = 0; i < domainWords.Length; i++)
            {
                if (i < 3)
                {
                    domainName += (domainWords[i] + "/");
                }
                else
                {
                    folderName += ("/" + domainWords[i]);
                }
            }

            result[0] = domainName;
            result[1] = folderName;

            return result;
        }
    }
}
