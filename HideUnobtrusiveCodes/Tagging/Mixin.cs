using System.Linq;
using Microsoft.VisualStudio.Text;
using static HideUnobtrusiveCodes.Application.App;

namespace HideUnobtrusiveCodes.Tagging
{
    /// <summary>
    ///     The mixin
    /// </summary>
    static class Mixin
    {
        #region Public Methods
        /// <summary>
        ///     Determines whether the specified a has intersection.
        /// </summary>
        public static bool HasIntersection(NormalizedSnapshotSpanCollection a, NormalizedSnapshotSpanCollection b)
        {
            return SafeExecute(() => a.IntersectsWith(b));
        }

        /// <summary>
        ///     Determines whether [is intersect with disabled spans] [the specified scope].
        /// </summary>
        public static bool IsIntersectWithDisabledSpans(AdornmentTaggerScope scope, SnapshotSpan span)
        {
            if (scope.DisabledSnapshotSpans.Any(x => IntersectsWith(x, span)))
            {
                return true;
            }

            if (scope.EditedSpans != null)
            {
                if (scope.EditedSpans.Any(x => IntersectsWith(x, span)))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Intersectses the with.
        /// </summary>
        static bool IntersectsWith(SnapshotSpan a, SnapshotSpan b)
        {
            return SafeExecute(() =>
            {
                a = ToLine(a);
                b = ToLine(b);

                return a.IntersectsWith(b);
            });
        }

        /// <summary>
        ///     To the line.
        /// </summary>
        static SnapshotSpan ToLine(SnapshotSpan snapshotSpan)
        {
            var textSnapshotLine = snapshotSpan.Snapshot.GetLineFromPosition(snapshotSpan.Start.Position);

            var startPosition = textSnapshotLine.Start.Position;

            var endPosition = snapshotSpan.Snapshot.GetLineFromPosition(snapshotSpan.End.Position).End;

            return new SnapshotSpan(snapshotSpan.Snapshot, startPosition, endPosition - startPosition);
        }
        #endregion
    }
}