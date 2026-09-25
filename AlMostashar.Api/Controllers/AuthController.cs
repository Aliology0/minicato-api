using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Auth.Commands.ForgotPassword;
using AlMostashar.Application.Features.Auth.Commands.Login;
using AlMostashar.Application.Features.Auth.Commands.RefreshToken;
using AlMostashar.Application.Features.Auth.Commands.RegisterClient;
using AlMostashar.Application.Features.Auth.Commands.RegisterLawyer;
using AlMostashar.Application.Features.Auth.Commands.ResendVerification;
using AlMostashar.Application.Features.Auth.Commands.ResetPassword;
using AlMostashar.Application.Features.Auth.Commands.UploadIdentityDocuments;
using AlMostashar.Application.Features.Auth.Commands.VerifyEmail;
using AlMostashar.Application.Features.Auth.Commands.VerifyOtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Register a new Client account. Sends OTP to email for verification.</summary>
        [HttpPost("register/client")]
        public async Task<IActionResult> RegisterClient(
            [FromBody] RegisterClientCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Verify client email with OTP code. Returns tokens on success.</summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(
            [FromBody] VerifyEmailCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Resend email verification OTP.</summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(
            [FromBody] ResendVerificationCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Register a new Lawyer account.</summary>
        [HttpPost("register/lawyer")]
        public async Task<IActionResult> RegisterLawyer(
            [FromBody] RegisterLawyerCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Login with email and password.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Exchange a valid refresh token for a new access token + rotated refresh token.</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Request a password reset OTP sent to email.</summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Verify the OTP code and get a short-lived reset token.</summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Reset password using the reset token from verify-otp.</summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Upload identity documents (SSN, Syndicate Card, Practice Certificates) to S3 storage.</summary>
        [HttpPost("upload-identity-documents")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadIdentityDocuments(
            [FromForm] UploadIdentityDocumentsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}


