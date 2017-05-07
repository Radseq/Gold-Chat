using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gold_Client.Model
{
    interface IClient
    {
        Client User { get; set; }
    }
}
