using ClosedXML.Excel;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class ComparisonExportService
    {
        /// <summary>
        /// 比較結果を色付きExcelファイルとしてエクスポートします
        /// </summary>
        public async Task ExportComparisonToColoredExcelAsync(ComparisonResult comparisonResult, string filePath)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("比較結果");

                // ヘッダー設定
                worksheet.Cell("A1").Value = "比較元";
                worksheet.Cell("B1").Value = "比較先";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("B1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell("B1").Style.Fill.BackgroundColor = XLColor.LightGray;

                // 列の幅を設定
                worksheet.Column("A").Width = 50;
                worksheet.Column("B").Width = 50;

                // データを書き込み
                int leftRow = 2;
                int rightRow = 2;

                WriteComparisonTreeToExcel(worksheet, comparisonResult.LeftTreeItems, "A", ref leftRow, true);
                WriteComparisonTreeToExcel(worksheet, comparisonResult.RightTreeItems, "B", ref rightRow, false);

                // セル境界線を設定
                var usedRange = worksheet.RangeUsed();
                if (usedRange != null)
                {
                    usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                workbook.SaveAs(filePath);
            });
        }

        /// <summary>
        /// 比較結果のツリーをExcelワークシートに書き込みます
        /// </summary>
        private void WriteComparisonTreeToExcel(IXLWorksheet worksheet, IEnumerable<FileSystemEntry> entries, string columnLetter, ref int currentRow, bool isLeft)
        {
            WriteComparisonTreeToExcel(worksheet, entries, columnLetter, ref currentRow, isLeft, new List<bool>());
        }

        /// <summary>
        /// 比較結果のツリーをツリー形式でExcelワークシートに書き込みます
        /// </summary>
        private void WriteComparisonTreeToExcel(IXLWorksheet worksheet, IEnumerable<FileSystemEntry> entries, string columnLetter, ref int currentRow, bool isLeft, List<bool> parentIsLast)
        {
            var entryList = entries.ToList();

            foreach (var (entry, index) in entryList.Select((e, i) => (e, i)))
            {
                var isLast = index == entryList.Count - 1;
                var treePrefix = BuildTreePrefix(parentIsLast, isLast);

                var displayName = $"{treePrefix}{entry.Name}{(entry.IsDirectory ? "/" : "")}";
                var cell = worksheet.Cell($"{columnLetter}{currentRow}");
                cell.Value = displayName;

                // 差分タイプに応じて色を設定
                ApplyDifferenceTypeColor(cell, entry.DifferenceType, isLeft);

                currentRow++;

                // フォルダの場合は子要素を処理
                if (entry.IsDirectory && entry.Children.Any())
                {
                    var newParentIsLast = new List<bool>(parentIsLast) { isLast };
                    WriteComparisonTreeToExcel(worksheet, entry.Children, columnLetter, ref currentRow, isLeft, newParentIsLast);
                }
            }
        }

        /// <summary>
        /// ツリー構造のプレフィックスを構築します
        /// </summary>
        /// <param name="parentIsLast">親階層での各要素が最後の要素かどうか</param>
        /// <param name="isLast">現在の要素が最後の要素かどうか</param>
        /// <returns>ツリー構造のプレフィックス文字列</returns>
        private string BuildTreePrefix(List<bool> parentIsLast, bool isLast)
        {
            var prefix = new System.Text.StringBuilder();

            // 親階層のプレフィックスを構築
            foreach (var parentLast in parentIsLast)
            {
                if (parentLast)
                {
                    prefix.Append("    "); // 最後の要素の子は空白4文字
                }
                else
                {
                    prefix.Append("│   "); // 中間要素の子は縦線
                }
            }

            // 現在の要素のプレフィックス
            if (parentIsLast.Count > 0) // ルート要素でない場合
            {
                prefix.Append(isLast ? "└─ " : "├─ ");
            }

            return prefix.ToString();
        }

        /// <summary>
        /// 差分タイプに応じてセルの色を設定します
        /// </summary>
        private void ApplyDifferenceTypeColor(IXLCell cell, DifferenceType differenceType, bool isLeft)
        {
            // デザートテーマに調和した色設定
            var unchangedBackgroundColor = XLColor.FromArgb(255, 250, 239); // #FFFAEF (デザートテーマのベース色)
            var unchangedForegroundColor = XLColor.FromArgb(61, 61, 61);    // #3D3D3D (デザートテーマのテキスト色)
            var subtleBackgroundColor = XLColor.FromArgb(248, 246, 242);    // #F8F6F2 (薄いベージュ)
            var subtleForegroundColor = XLColor.FromArgb(192, 181, 160);    // #C0B5A0 (薄いテキスト色)

            switch (differenceType)
            {
                case DifferenceType.Added:
                    // 追加: 明るい緑
                    cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    cell.Style.Font.FontColor = XLColor.DarkGreen;
                    break;
                case DifferenceType.Deleted:
                    // 削除: 明るい赤
                    cell.Style.Fill.BackgroundColor = XLColor.LightCoral;
                    cell.Style.Font.FontColor = XLColor.DarkRed;
                    break;
                case DifferenceType.Modified:
                    // 変更: 明るい黄色
                    cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    cell.Style.Font.FontColor = XLColor.DarkOrange;
                    break;
                case DifferenceType.Unchanged:
                    // 変更なし: デザートテーマの通常色
                    cell.Style.Fill.BackgroundColor = unchangedBackgroundColor;
                    cell.Style.Font.FontColor = unchangedForegroundColor;
                    break;
                case DifferenceType.None:
                default:
                    // デフォルト: 薄い色で控えめに表示
                    cell.Style.Fill.BackgroundColor = subtleBackgroundColor;
                    cell.Style.Font.FontColor = subtleForegroundColor;
                    break;
            }
        }
    }
}