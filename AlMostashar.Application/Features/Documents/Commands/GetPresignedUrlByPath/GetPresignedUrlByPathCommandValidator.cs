using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Documents.Commands.GetPresignedUrlByPath
{
    public class GetPresignedUrlByPathCommandValidator : AbstractValidator<GetPresignedUrlByPathCommand>
    {
        public GetPresignedUrlByPathCommandValidator()
        {
            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage(Messages.Generic.Required(Messages.Fields.FilePath));
        }
    }
}
