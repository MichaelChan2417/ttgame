using System;

namespace Bordy
{
    /// <summary>
    /// Daily challenge state. The puzzle template is a fixed shared layout from
    /// <see cref="BordyLevelCatalog"/> (same board for every player). A player may keep
    /// attempting today's puzzle until they solve it; once solved, the result (final board +
    /// time) is stored and re-entering shows a read-only result page for the rest of the day.
    /// Boundary is UTC midnight. State persists through <see cref="BordyStore"/>.
    ///
    /// 每日挑战状态。题目模板是 <see cref="BordyLevelCatalog"/> 里的固定共享布局（所有玩家同一盘）。
    /// 玩家当天可一直尝试直到解出；解出后保存成绩（最终盘面 + 用时），当天再次进入即显示只读
    /// 结算页。以 UTC 零点为界。状态通过 <see cref="BordyStore"/> 持久化。
    /// </summary>
    public static class BordyDaily
    {
        private const string DateKey = "bordy.daily.date";     // yyyyMMdd of completion / 通关日期
        private const string TimeKey = "bordy.daily.seconds";  // elapsed seconds at solve / 通关用时
        private const string BoardKey = "bordy.daily.board";   // final board, row-major '0'/'1' / 最终盘面

        // In-progress snapshot (resume where you left off). Board uses '0'=sun '1'=moon '2'=empty.
        // 进行中存档（断点续玩）。盘面用 '0'=太阳 '1'=月亮 '2'=空。
        private const string ProgDateKey = "bordy.daily.prog.date";
        private const string ProgBoardKey = "bordy.daily.prog.board";
        private const string ProgTimeKey = "bordy.daily.prog.seconds";

        /// <summary>Today's key in UTC, e.g. "20260613". / 今天的 UTC 日期键。</summary>
        public static string TodayKey => DateTime.UtcNow.ToString("yyyyMMdd");

        /// <summary>True once the player has solved today's daily. / 玩家已解出今天的每日挑战。</summary>
        public static bool CompletedToday => BordyStore.GetString(DateKey, "") == TodayKey;

        /// <summary>Recorded solve time (seconds) for today. / 今天记录的通关用时（秒）。</summary>
        public static int CompletedSeconds => BordyStore.GetInt(TimeKey, 0);

        /// <summary>Recorded final board (row-major '0'=sun '1'=moon), or empty. / 记录的最终盘面。</summary>
        public static string CompletedBoard => BordyStore.GetString(BoardKey, "");

        /// <summary>Store today's result on solve. / 通关时保存今天的成绩。</summary>
        public static void SaveResult(int seconds, string board)
        {
            BordyStore.SetString(DateKey, TodayKey);
            BordyStore.SetInt(TimeKey, seconds);
            BordyStore.SetString(BoardKey, board);
            BordyStore.Save();
            BordyCloudSync.PushNow();
        }

        // --- In-progress snapshot (resume) ---

        /// <summary>True if there's a saved in-progress board for today. / 今天有进行中存档。</summary>
        public static bool HasProgressToday =>
            BordyStore.GetString(ProgDateKey, "") == TodayKey &&
            !string.IsNullOrEmpty(BordyStore.GetString(ProgBoardKey, ""));

        public static string ProgressBoard => BordyStore.GetString(ProgBoardKey, "");
        public static int ProgressSeconds => BordyStore.GetInt(ProgTimeKey, 0);

        /// <summary>Save the current in-progress board + elapsed gameplay seconds for today. / 保存今天进行中的盘面与已用时。</summary>
        public static void SaveProgress(int seconds, string board)
        {
            BordyStore.SetString(ProgDateKey, TodayKey);
            BordyStore.SetString(ProgBoardKey, board);
            BordyStore.SetInt(ProgTimeKey, seconds);
            BordyStore.Save();
            BordyCloudSync.PushDebounced();
        }

        /// <summary>Drop the in-progress snapshot (on reset or after solving). / 清掉进行中存档（重置或通关后）。</summary>
        public static void ClearProgress()
        {
            BordyStore.DeleteKey(ProgDateKey);
            BordyStore.DeleteKey(ProgBoardKey);
            BordyStore.DeleteKey(ProgTimeKey);
            BordyStore.Save();
            BordyCloudSync.PushDebounced();
        }

        /// <summary>Apply server daily fields to local store. / 把云端每日数据写入本地。</summary>
        public static void ApplyFromCloud(BordyCloudDailySave cloud)
        {
            if (cloud == null) return;

            BordyStore.SetString(DateKey, cloud.completedDate ?? "");
            BordyStore.SetInt(TimeKey, cloud.completedSeconds);
            BordyStore.SetString(BoardKey, cloud.completedBoard ?? "");
            BordyStore.SetString(ProgDateKey, cloud.progressDate ?? "");
            BordyStore.SetString(ProgBoardKey, cloud.progressBoard ?? "");
            BordyStore.SetInt(ProgTimeKey, cloud.progressSeconds);
            BordyStore.Save();
        }

        /// <summary>Snapshot for cloud upload. / 采集每日数据用于上传。</summary>
        public static BordyCloudDailySave CaptureForCloud()
        {
            return new BordyCloudDailySave
            {
                completedDate = BordyStore.GetString(DateKey, ""),
                completedSeconds = BordyStore.GetInt(TimeKey, 0),
                completedBoard = BordyStore.GetString(BoardKey, ""),
                progressDate = BordyStore.GetString(ProgDateKey, ""),
                progressBoard = BordyStore.GetString(ProgBoardKey, ""),
                progressSeconds = BordyStore.GetInt(ProgTimeKey, 0),
            };
        }

        /// <summary>Clear all daily records (testing). / 清除全部每日记录（测试用）。</summary>
        public static void Reset()
        {
            BordyStore.DeleteKey(DateKey);
            BordyStore.DeleteKey(TimeKey);
            BordyStore.DeleteKey(BoardKey);
            ClearProgress();
        }
    }
}
