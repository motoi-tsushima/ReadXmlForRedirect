using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadXmlForRedirect
{
    internal class ConstSettings
    {
        public const string Message = "[ReadXmlForRidirect]  Copyright © 2023 motoi.tsushima .  ( help option -> /h or /? )";
        public const string Help = "<Help>\n(1) ブログのエクスポートXMLファイルを読み取るときは、/b:[ブログ種類] [XMLファイル名] を指定する。\n オプション種類:  /b:blogger , /b:b , /b:wordpress , /b:w \n\n 例: ReadXmlForRedirect /b:b exfile.xml \n\n(2) リダイレクト設定ファイルを作るときは、 /r:[出力形式名] [入力XMLファイル名] [出力XMLファイル名] を指定する。\n オプション種類:  /r:csv , /r:redirect , /r:rewriterule \n\n 例: ReadXmlForRedirect /r:csv inputfile.csv outputfile.csv ";
    }
}
