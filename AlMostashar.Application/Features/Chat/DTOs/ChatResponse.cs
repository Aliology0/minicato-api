using AlMostashar.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Chat.DTOs
{
    public class ChatResponse : CursorPagedResult<ChatLookupDto>
    {
        public DateTime? CursorDate { get; set; }
    }
}