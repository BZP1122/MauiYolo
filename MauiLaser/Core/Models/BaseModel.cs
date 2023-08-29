using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Honor_Maui.Core.Models
{
    public partial class BaseModel: ObservableObject
    {
        [ObservableProperty]
        long id;
    }
}
