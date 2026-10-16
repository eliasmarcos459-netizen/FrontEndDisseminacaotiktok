using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMAD
{
    public static class SessaoThemeMudBlazor
    {
        public static bool _IsDarkTheme { get; set; } = false;

        public static MudTheme _customTheme = new()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#1565C0",
                Secondary = "#546E7A",
                Tertiary = "#03A9F4",
                AppbarBackground = "#1565C0",
                Surface = "#FFFFFF",
                Background = "#F8FAFD",
                Divider = "rgba(0,0,0,0.05)"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#2196F3",
                Secondary = "#90A4AE",
                Tertiary = "#4FC3F7",
                Surface = "#161b22",
                Background = "#0d1117",
                AppbarBackground = "#0d1117",
                TextPrimary = "#E6edf3",
                Divider = "rgba(255,255,255,0.1)"
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "16px"
            }
        };

        public static Dictionary<string, string> coresPartidos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "MPLA", "#E30613" },                 // Vermelho
    { "UNITA", "#2E7D32" },                // Verde (Cor representativa da UNITA)
    { "CASA-CE", "#FF6600" },              // Laranja
    { "PRS", "#004D40" },                  // Verde escuro / Petróleo (para diferenciar do verde da UNITA)
    { "FNLA", "#003399" },                 // Azul
    { "PHA", "#9900CC" },                  // Roxo
    { "APN", "#FFCC00" },                  // Amarelo
    { "P-NJANGO", "#00ACC1" },             // Azul Ciano
    { "PL", "#795548" },                   // Castanho
    { "PDP-ANA", "#607D8B" },              // Azul Cinzento
    { "PADDA-AP", "#E91E63" },             // Rosa
    { "RA", "#8BC34A" },                   // Verde Claro
    { "PNSA", "#3F51B5" },                 // Índigo
    { "PPA", "#FF5722" },                  // Laranja escuro
    { "BD", "#009688" },                   // Verde Água
    { "CIDADANIA", "#9C27B0" },            // Magenta
    { "PRA-JA SERVIR ANGOLA", "#1E88E5" }  // Azul
};
    }
}
