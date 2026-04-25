using HackYourWay.Models;

namespace HackYourWay.Services
{
    /// <summary>
    /// Calculates the effective execution time for terminal commands based on player
    /// hardware tiers and (for network commands) the target device's bandwidth tier.
    /// Uses the bottleneck model: effective network speed is
    /// <c>min(player.BandwidthTier, target.BandwidthTier)</c> for all transfer commands.
    /// Crack uses CPU × GPU speedup only — no network component.
    /// </summary>
    public class CommandLatencyService
    {
        // ── Coefficient tables ──────────────────────────────────────────────────
        // All tables are 0-indexed by (tier − 1), except GpuSpeedupTable (0-indexed by tier).

        private static readonly float[] CpuSpeedupTable =
            { 1.0f, 1.5f, 2.5f, 3.5f, 5.0f };

        /// <summary>Shared by player bandwidth and target bandwidth lookups.</summary>
        public static readonly float[] BandwidthSpeedupTable =
            { 1.0f, 1.5f, 2.3f, 3.2f, 4.0f };

        private static readonly float[] GpuSpeedupTable =
            { 1.0f, 1.6f, 2.8f, 4.0f };

        // ── Per-command base times and caps ─────────────────────────────────────

        private const float BaseCrackWep  = 3.0f;  private const float MaxCrackWep  = 10.0f;
        private const float BaseCrackWpa  = 6.0f;  private const float MaxCrackWpa  = 20.0f;
        private const float BaseCrackWpa2 = 10.0f; private const float MaxCrackWpa2 = 30.0f;
        private const float BaseAreaScan  = 1.5f;  private const float MaxAreaScan  = 8.0f;
        private const float BaseIpScan    = 2.0f;  private const float MaxIpScan    = 8.0f;
        private const float BaseInject    = 4.0f;  private const float MaxInject    = 15.0f;
        private const float BaseFirewall  = 2.5f;  private const float MaxFirewall  = 12.0f;
        private const float BaseLs        = 0.5f;  private const float MaxLs        = 8.0f;
        private const float BaseCopy      = 3.0f;  private const float MaxCopy      = 15.0f;

        // ── Floor and copy scaling ──────────────────────────────────────────────

        /// <summary>Minimum effective latency in seconds; no command executes faster than this.</summary>
        public const float MinFloor = 0.05f;

        private const long  CopyReferenceSizeBytes = 10_000_000L;
        private const float CopyScaleFactor        = 2.0f;

        // ── State ───────────────────────────────────────────────────────────────

        private readonly Player _player;

        /// <summary>Constructs the service bound to the current session's player.</summary>
        public CommandLatencyService(Player player)
        {
            _player = player;
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the effective execution time in seconds for the given command context.
        /// Result is clamped to [<see cref="MinFloor"/>, per-command maximum].
        /// Returns 0 for unknown or instant verbs.
        /// </summary>
        public float CalculateLatency(CommandLatencyContext ctx)
        {
            switch (ctx.CommandVerb)
            {
                case "crack":    return CalculateCrack(ctx.SecurityLevel);
                case "scan":     return ctx.TargetDevice == null
                                     ? CalculateAreaScan()
                                     : CalculateNetwork(BaseIpScan, MaxIpScan, ctx.TargetDevice);
                case "inject":   return CalculateNetwork(BaseInject,   MaxInject,   ctx.TargetDevice);
                case "firewall": return CalculateNetwork(BaseFirewall, MaxFirewall, ctx.TargetDevice);
                case "ls":       return CalculateNetwork(BaseLs,       MaxLs,       ctx.TargetDevice);
                case "copy":     return CalculateCopy(ctx.TargetDevice, ctx.FileSizeBytes);
                default:         return 0f;
            }
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private float CalculateCrack(SecurityLevel level)
        {
            float baseSec, maxSec;
            switch (level)
            {
                case SecurityLevel.WEP:  baseSec = BaseCrackWep;  maxSec = MaxCrackWep;  break;
                case SecurityLevel.WPA:  baseSec = BaseCrackWpa;  maxSec = MaxCrackWpa;  break;
                default:                 baseSec = BaseCrackWpa2; maxSec = MaxCrackWpa2; break;
            }

            float cpu = CpuSpeedupTable[Clamp(_player.CpuTier - 1, 0, 4)];
            float gpu = GpuSpeedupTable[Clamp(_player.GpuTier,     0, 3)];
            return Clamp(baseSec / (cpu * gpu), MinFloor, maxSec);
        }

        private float CalculateAreaScan()
        {
            float speedup = BandwidthSpeedupTable[Clamp(_player.BandwidthTier - 1, 0, 4)];
            return Clamp(BaseAreaScan / speedup, MinFloor, MaxAreaScan);
        }

        private float CalculateNetwork(float baseSec, float maxSec, Device target)
        {
            int targetBw = target != null ? target.BandwidthTier : 1;
            int effTier  = System.Math.Min(_player.BandwidthTier, targetBw);
            float speedup = BandwidthSpeedupTable[Clamp(effTier - 1, 0, 4)];
            return Clamp(baseSec / speedup, MinFloor, maxSec);
        }

        private float CalculateCopy(Device target, long fileSizeBytes)
        {
            float scale    = 1.0f + fileSizeBytes / (float)CopyReferenceSizeBytes * CopyScaleFactor;
            float adjusted = BaseCopy * scale;

            int targetBw  = target != null ? target.BandwidthTier : 1;
            int effTier   = System.Math.Min(_player.BandwidthTier, targetBw);
            float speedup = BandwidthSpeedupTable[Clamp(effTier - 1, 0, 4)];
            return Clamp(adjusted / speedup, MinFloor, MaxCopy);
        }

        private static int   Clamp(int   v, int   lo, int   hi) => v < lo ? lo : v > hi ? hi : v;
        private static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }

    /// <summary>
    /// Immutable context passed to <see cref="CommandLatencyService.CalculateLatency"/>.
    /// Only populate the fields relevant to the command verb being queried.
    /// </summary>
    public readonly struct CommandLatencyContext
    {
        /// <summary>Command verb (e.g. "crack", "scan", "inject", "copy").</summary>
        public readonly string        CommandVerb;

        /// <summary>Security level — meaningful for "crack" only; ignored otherwise.</summary>
        public readonly SecurityLevel SecurityLevel;

        /// <summary>
        /// Target device — null signals area scan (no target) or an unresolvable target.
        /// A null target causes network commands to use tier-1 bandwidth (worst case).
        /// </summary>
        public readonly Device        TargetDevice;

        /// <summary>File size in bytes — meaningful for "copy" only; 0 otherwise.</summary>
        public readonly long          FileSizeBytes;

        /// <summary>Constructs a fully specified context.</summary>
        public CommandLatencyContext(
            string verb,
            SecurityLevel secLevel    = default,
            Device        target      = null,
            long          fileSize    = 0)
        {
            CommandVerb   = verb;
            SecurityLevel = secLevel;
            TargetDevice  = target;
            FileSizeBytes = fileSize;
        }
    }
}
