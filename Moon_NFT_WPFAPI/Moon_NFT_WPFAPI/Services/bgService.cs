namespace Moon_NFT_WPFAPI.Services
{
    public static class bgService
    {
        public static System.Drawing.Color HexToColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("HEX строка не может быть пустой.");

            // Удаляем '#' если есть
            hex = hex.TrimStart('#');

            // Поддержка короткого формата #RGB → #RRGGBB
            if (hex.Length == 3)
            {
                hex = new string(new char[]
                {
                    hex[0], hex[0],
                    hex[1], hex[1],
                    hex[2], hex[2]
                });
            }

            if (hex.Length != 6)
                throw new ArgumentException("Некорректный формат HEX. Ожидается #RRGGBB или #RGB.");

            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}
