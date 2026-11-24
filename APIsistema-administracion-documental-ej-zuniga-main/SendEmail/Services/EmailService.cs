using SendEmail.Models;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace SendEmail.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _emailUserName;
        private readonly string _emailPassword;
        private readonly string _emailHost;
        private readonly int _emailPort;

        public EmailService(IConfiguration config)
        {
            _emailUserName = config["Email:UserName"]?.Trim() ?? string.Empty;
            _emailPassword = config["Email:PassWord"]?.Trim() ?? string.Empty;
            _emailHost = config["Email:Host"]?.Trim() ?? string.Empty;

            var portTexto = config["Email:Port"]?.Trim();
            if (!int.TryParse(portTexto, out _emailPort))
            {
                _emailPort = 587;
            }
        }

        private bool EstaConfigurado()
        {
            return !string.IsNullOrWhiteSpace(_emailUserName)
                && !string.IsNullOrWhiteSpace(_emailPassword)
                && !string.IsNullOrWhiteSpace(_emailHost)
                && _emailPort > 0;
        }

        private MimeMessage CrearMensaje(EmailDTO request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Para))
            {
                throw new ArgumentException("Debe especificar el destinatario del correo.", nameof(request.Para));
            }

            if (string.IsNullOrWhiteSpace(request.Asunto))
            {
                throw new ArgumentException("Debe especificar el asunto del correo.", nameof(request.Asunto));
            }

            if (string.IsNullOrWhiteSpace(request.Contenido))
            {
                throw new ArgumentException("Debe especificar el contenido del correo.", nameof(request.Contenido));
            }

            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_emailUserName));
            email.To.Add(MailboxAddress.Parse(request.Para));
            email.Subject = request.Asunto;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = request.Contenido
            };

            return email;
        }

        public void SendEmail(EmailDTO request)
        {
            if (!EstaConfigurado())
                throw new InvalidOperationException("El servicio de correo no está configurado.");
            var email = CrearMensaje(request);

            using var smtp = new SmtpClient();

            smtp.Connect(
                _emailHost,
                _emailPort,
                SecureSocketOptions.StartTls
            );

            smtp.Authenticate(_emailUserName, _emailPassword);

            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public async Task<bool> EnviarAsync(EmailDTO emailDTO)
        {
            if (!EstaConfigurado()) return false;
            try
            {
                var email = CrearMensaje(emailDTO);

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _emailHost,
                    _emailPort,
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(_emailUserName, _emailPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Enviar(EmailDTO emailDTO)
        {
            if (!EstaConfigurado()) return false;
            try
            {
                var email = CrearMensaje(emailDTO);

                using var smtp = new SmtpClient();

                smtp.Connect(
                    _emailHost,
                    _emailPort,
                    SecureSocketOptions.StartTls
                );

                smtp.Authenticate(_emailUserName, _emailPassword);

                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}