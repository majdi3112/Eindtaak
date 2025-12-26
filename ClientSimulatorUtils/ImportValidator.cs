using System;
using System.Collections.Generic;

namespace ClientSimulatorUtils
{
    public static class ImportValidator
    {
        private static readonly HashSet<string> OngeldigeGemeenteMarkers = new(StringComparer.OrdinalIgnoreCase)
        {
            "(unknown)", "unknown", "<unknown>", "[unknown]",
            "null", "(null)", "undefined", "n/a", "N/A",
            "-", "—", "0", "?", ""
        };

        public static bool IsOngeldigeGemeente(string g)
        {
            if (string.IsNullOrWhiteSpace(g))
                return true;

            string clean = g.Trim();

            return OngeldigeGemeenteMarkers.Contains(clean);
        }


        // Alle relevante OSM highway types
        private static readonly HashSet<string> GeldigeWegTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "residential",
            "primary",
            "secondary",
            "tertiary",
            "service",
            "motorway",
            "unclassified",
            "trunk",
            "living_street",
            "road",
            "track",
            "path",
            "cycleway",
            "footway"
        };

        public static bool IsGeldigWegtype(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return false;

            return GeldigeWegTypes.Contains(type.Trim());
        }


        public static bool KolomBestaat(string[] row, int index)
        {
            if (row == null || row.Length <= index)
                return false;

            return !string.IsNullOrWhiteSpace(row[index]);
        }
    }
}
