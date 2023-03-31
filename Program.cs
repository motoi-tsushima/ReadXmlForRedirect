using System;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace ReadXmlForRedirect
{
    public class Program
    {
        static void Main(string[] args)
        {
            //コマンドラインのパラメータを設定する。
            CommandOptions command = new(args);

            //コマンドモード(/b)を選択する。
            if(command.IsMode() == CommandOptions.MODE_SHOW_NON)
            {
                Console.WriteLine(command.Message);
                //エラー
                //Console.WriteLine("/b:ブログ規格名   を指定してください。 例として、 /b:blogger , /b:wordpress など。");
            }
            else if(command.IsMode() == CommandOptions.MODE_READ_XML)
            {
                //エクスポートxmlファイルの読み込みモード。
                string xmlFilePath = command.Parameters[0];

                ReadXmlElements readXmlElements = new ReadXmlElements();

                if (command.BlogStandard == CommandOptions.BLOG_STANDARD_BLOGGER)
                {
                    //Blogger のエクスポートファイルを読み込む。(結果はCSVで出力する)
                    readXmlElements.ReadBloggerXml(xmlFilePath);
                }
                else if (command.BlogStandard == CommandOptions.BLOG_STANDARD_WORDPRESS)
                {
                    //Wordpress のエクスポートファイルを読み込む。(結果はCSVで出力する)
                    readXmlElements.ReadWordpressXml(xmlFilePath);
                }
            }
            else if(command.IsMode() == CommandOptions.MODE_READ_CSV)
            {
                //CSVファイルの読み込みモード。
                ReadXmlElements readXmlElements = new ReadXmlElements();

                //リダイレクト設定を出力する。
                readXmlElements.MakeRedirectFile(command.InputCSVFilePath, command.OutputCSVFilePath, command.RedirectFileMode);
            }
            else if(command.IsMode() == CommandOptions.MODE_SHOW_HELP)
            {
                Console.WriteLine(command.Message);
                Console.WriteLine(command.Help);
            }
        }
    }
}