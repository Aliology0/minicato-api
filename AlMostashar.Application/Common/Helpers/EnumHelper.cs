using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlMostashar.Application.Common.Helpers;

public static class EnumHelper
{
    public static IEnumerable<EnumDto> GetEnumList<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Select(e => new EnumDto(
            Convert.ToInt32(e),
            e.ToString(),
            Messages.Get($"Enum_{typeof(T).Name}_{e}")
        ));
    }
}
