using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace XUnit.Core.Policy
{
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var role = context.User.FindFirst(c => c.Type == ClaimTypes.Role);
            if (role != null)
            {
                var roleValue = role.Value;
                if (roleValue==requirement._permissionName)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
    ////权限动态缓存类 临时替代数据库
    //public class RolePermissionCache
    //{
    //    //实际在数据库获取与配置
    //    public static List<string> GetPermissions(string role)
    //    {
    //        switch (role)
    //        {
    //            case "admin":
    //                return new List<string>() { "Index", "Privacy" };
    //            case "user":
    //                return new List<string>() { "Index" };
    //        }
    //        return new List<string>();
    //    }
    //}
}
