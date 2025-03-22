﻿using System.Drawing;
using System.Drawing.Imaging;

namespace ZuvoPet_V2.Helpers
{
    public class HelperAvatarDinamico
    {
        public static string GetIniciales(string nombre)
        {
            string[] palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string iniciales = "";

            foreach (var palabra in palabras)
            {
                iniciales += char.ToUpper(palabra[0]);

                if (iniciales.Length == 2) break;
            }

            return iniciales;
        }

        public static byte[] GenerarAvatar(string iniciales)
        {
            int ancho = 150, alto = 150;

            Bitmap bitmap = new Bitmap(ancho, alto);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Color color = GenerarColorAleatorio();

                g.Clear(color);

                Font fuente = new Font("Arial", 50, FontStyle.Bold);

                Brush textoBlanco = Brushes.White;

                SizeF tamano = g.MeasureString(iniciales, fuente);

                float x = (ancho - tamano.Width) / 2;
                float y = (alto - tamano.Height) / 2;

                g.DrawString(iniciales, fuente, textoBlanco, x, y);
            }

            using MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);

            return ms.ToArray();
        }

        public static Color GenerarColorAleatorio()
        {
            Random rand = new Random();
            while (true)
            {
                // Generar un color aleatorio
                Color color = Color.FromArgb(rand.Next(100, 256), rand.Next(100, 256), rand.Next(100, 256));

                // Calcular la luminosidad perceptual
                double luminosidad = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

                // Calcular el contraste con el texto blanco (#FFFFFF)
                double contrasteBlanco = Math.Abs(1.0 - luminosidad);

                // Asegurar que el color sea suficientemente brillante pero no excesivamente claro
                if (luminosidad >= 0.5 && luminosidad <= 0.8 && contrasteBlanco >= 0.5)
                    return color;
            }
        }


        public static string CrearYGuardarAvatar(string nombreUsuario)
        {
            string iniciales = HelperAvatarDinamico.GetIniciales(nombreUsuario);
            byte[] imagenAvatar = HelperAvatarDinamico.GenerarAvatar(iniciales);

            string carpetaAvatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(carpetaAvatar))
            {
                Directory.CreateDirectory(carpetaAvatar);
            }

            string nombreAvatar = $"{Guid.NewGuid()}.png";
            string nombreArchivo = Path.Combine(carpetaAvatar, nombreAvatar);
            System.IO.File.WriteAllBytes(nombreArchivo, imagenAvatar);

            return nombreAvatar;
        }

    }
}
