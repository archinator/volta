﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Volta.Bot.Application.Domain;

namespace Volta.Bot.Application.Interfaces
{
    public interface IResourceHandler
    {
        Task Handle(Message message);
    }
}
