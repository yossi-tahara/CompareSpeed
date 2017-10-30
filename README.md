
リンク：[【Theolizer公式サイト】](https://theolizer.com)[【サポート掲示板】](https://theolizer.com/customer/forums/)

Qiitaの記事[C#とC++の速度比較をちょっとまじめにやってみた](https://qiita.com/Chironian/items/e8c9f0147669f941936a)で用いたソース・コード一式です。

詳しい評価目的はTheolizerのIssues #42[メタ・シリアライズによるC#連携機能の追加について](https://github.com/yossi-tahara/Theolizer/issues/42)を参照下さい。

<strong>簡単な使い方説明：</strong><br>
ビルドするとmeasure_cpp.exeとmeasure_cs.exeがbin/$(Configulation)の下に生成されます。
コマンド・ライン・パラメータ無しなら1回比較します。
パラメータは１つだけ有効で、3なら3回比較、100なら100回比較します。
標準エラー出力にcsv形式で出力します。これはリダイレクトしてcsvファイルへ保存し、表計算ツールで読むことを想定しています。

---
© 2016 [Theoride Technology](http://theolizer.com/) All Rights Reserved.  
["Theolizer" is a registered trademark of Theoride Technology.](http://theolizer.com/info/theolizer%E3%81%8C%E5%95%86%E6%A8%99%E7%99%BB%E9%8C%B2%E3%81%95%E3%82%8C%E3%81%BE%E3%81%97%E3%81%9F/)
