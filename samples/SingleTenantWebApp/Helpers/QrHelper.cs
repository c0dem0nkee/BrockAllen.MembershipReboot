using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Helpers {
    public static class QrHelper {
        public static IHtmlString QRCode(this HtmlHelper html, string content) {
            var enc = new QrEncoder(ErrorCorrectionLevel.H);
            var code = enc.Encode(content);

            var r = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);

            using (var ms = new MemoryStream()) {
                r.WriteToStream(code.Matrix, ImageFormat.Png, ms);

                var image = ms.ToArray();

                return html.Raw(string.Format(@"<img src=""data:image/png;base64,{0}"" alt=""{1}"" />", Convert.ToBase64String(image), content));
            }
        }
    }
}