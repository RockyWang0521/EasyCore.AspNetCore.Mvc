using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependencie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.AppService.Contracts
{
    [ConsulService("App1")]
    public interface IApp1ConsulTestAppService : IRemoteAppService, ITransientDependencie
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(int id);

        Task EasyCoreAppService();
    }
}
