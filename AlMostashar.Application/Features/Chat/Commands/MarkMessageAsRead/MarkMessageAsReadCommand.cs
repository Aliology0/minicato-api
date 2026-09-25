using AlMostashar.Domain.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Chat.Commands.MarkMessageAsRead
{
    public class MarkMessageAsReadCommand:IRequest<Result<string>>
    {
        public int ChatId { get; set; }
        public int? LastMessageId { get; set; }
    }
}
