using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static ReadXmlForRedirect.ReadXmlElements;
using Microsoft.Extensions.Configuration;

namespace ReadXmlForRedirect
{
    internal class CommandOptions : Colipex
    {
        public const int MODE_NON = 0;
        public const int MODE_ERROR = -1;
        public const int MODE_READ_XML = 1;
        public const int MODE_READ_CSV = 2;
        public const int MODE_SHOW_HELP = 101;

        public const string BLOG_STANDARD_BLOGGER = "blogger";
        public const string BLOG_STANDARD_WORDPRESS = "wordpress";

        public CommandOptions(string[] args) : base(args) { }


        private int mode = MODE_NON;
        public int Mode
        {
            get => this.mode;
        }

        private ReadXmlElements.RedirectFileMode redirectFileMode = RedirectFileMode.non;
        public ReadXmlElements.RedirectFileMode RedirectFileMode
        {
            get => this.redirectFileMode;
        }

        private string blogStandard = string.Empty;
        public string BlogStandard
        {
            get => this.blogStandard; 
        }

        private string inputCSVFilePath = string.Empty;
        public string InputCSVFilePath
        {
            get => this.inputCSVFilePath;
        }

        private string outputCSVFilePath = string.Empty;
        public string OutputCSVFilePath
        {
            get => this.outputCSVFilePath;
        }

        private string option = string.Empty;
        public string Option
        {
            get => this.option;
        }

        private string message = string.Empty;

        private string help = string.Empty;
        public string Help
        {
            get => this.help;
        }

        private bool InitPropertys()
        {
            if (this.mode == MODE_NON)
            {
                //設定ファイルを読み込むのは止めた。固定値はクラスで持つ。設定値を読み込む。
                this.message = ConstSettings.Message;
                this.help = ConstSettings.Help;

                //コマンド・オプションの確認と設定をする
                if (base.IsOption("b"))
                {
                    //--- blog の XMLファイルを読み込み、タイトルとURLをCSV出力する -----
                    blogStandard = base.Options["b"].ToLower();

                    if (blogStandard == "b")
                    {
                        //Blogger エクスポートXML 読み込みモード
                        blogStandard = BLOG_STANDARD_BLOGGER;
                    }
                    else if (blogStandard == "w")
                    {
                        //Wordpress エクスポートXML 読み込みモード
                        blogStandard = BLOG_STANDARD_WORDPRESS;
                    }

                    //エクスポートXML読み込みモード
                    this.mode = MODE_READ_XML;

                    Console.WriteLine(this.message);
                }
                else if (base.IsOption("r"))
                {
                    //--- XMLから出力した2つのCSVを読み込み、Rerirect用のURLとURLのCSVを出力する -----
                    this.mode = MODE_READ_CSV;

                    this.inputCSVFilePath = base.Parameters[0];
                    this.outputCSVFilePath = base.Parameters[1];
                    this.option = base.Options["r"].ToLower();

                    if (option == CommandOptions.NonValue.ToLower())
                    {
                        Console.WriteLine("/r:リダイレクトモードを指定してください。例:  /r:csv  /r:rewriterule  /r:redirect ");
                        return true;
                    }

                    //リダイレクト設定の出力形式を設定をする。
                    if (option == "csv")
                    {
                        //CSV形式で出力
                        redirectFileMode = ReadXmlElements.RedirectFileMode.CSV;
                    }
                    else if (option == "rewriterule")
                    {
                        //.htaccess RewriteRule 形式で出力
                        redirectFileMode = ReadXmlElements.RedirectFileMode.htaccess1;
                    }
                    else if (option == "redirect")
                    {
                        //.htaccess Redirect 形式で出力
                        redirectFileMode = ReadXmlElements.RedirectFileMode.htaccess2;
                    }
                    else
                    {
                        //CSV形式で出力(ありえないルート)
                        redirectFileMode = ReadXmlElements.RedirectFileMode.CSV;
                    }

                    Console.WriteLine(this.message);
                }
                else if(base.IsOption("h") || base.IsOption("?")) {

                    this.mode = MODE_SHOW_HELP;

                    Console.WriteLine(this.help);
                }
                else
                {
                    Console.WriteLine(this.message);

                    //エラー
                    Console.WriteLine("/b:ブログ規格名   を指定してください。 例として、 /b:blogger , /b:wordpress など。");
                    return true;
                }
            }

            return false;
        }

        public int IsMode()
        {
            if (this.InitPropertys())
            {
                //エラー
                return MODE_ERROR;
            }

            return this.Mode;
        }
    }
}
