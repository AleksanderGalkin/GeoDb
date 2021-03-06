﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Security;
using System.Threading;
using System.Security.Principal;
using GeoDB.Service.DataAccess;
using GeoDB.Presenter;
using Ninject;
using Ninject.Parameters;

namespace GeoDB.Service.Security
{

    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AnyRolePermissionAttribute : CodeAccessSecurityAttribute
    {
        public AnyRolePermissionAttribute(SecurityAction action)
            : base(action)
        {

        }

        public string Roles { get; set; }

        public override IPermission CreatePermission()
        {
            IList<string> roles = (this.Roles ?? string.Empty).Split(',', ';')
                                    .Select(s => s.Trim())
                                    .Where(s => s.Length > 0)
                                    .Distinct()
                                    .ToList();

            IPermission result;
            if (roles.Count == 0)
            {
                result = new PrincipalPermission(null, null, true);
            }
            else
            {
                result = new PrincipalPermission(null, roles[0]);
                for (int i = 1; i < roles.Count; i++)
                {
                    result = result.Union(new PrincipalPermission(null, roles[i]));
                }
            }

            return result;
        }

    }

    public static class ImperativeRolePermission
    {
        public static bool currentPrincipalInRoles(string Roles)
        {
            IList<string> roles = (Roles ?? string.Empty).Split(',', ';')
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .Distinct()
                        .ToList();

            IPermission result;
            if (roles.Count == 0)
            {
                result = new PrincipalPermission(null, null, true);
            }
            else
            {
                result = new PrincipalPermission(null, roles[0]);
                for (int i = 1; i < roles.Count; i++)
                {
                    result = result.Union(new PrincipalPermission(null, roles[i]));
                }
            }

            try
            {
                result.Demand();
            }
            catch (SecurityException securityEx)
            {
                string messageInner = securityEx.InnerException != null ? securityEx.InnerException.ToString() : "";
                string message = "Ошибка безопасности" + Environment.NewLine + securityEx.Message + Environment.NewLine + messageInner;
                PException pexception = StaticInformation.ninjectKernel.Get<PException>(new ConstructorArgument("MessageText", message, false));
                pexception.Show();
                return false;
            }
            return true;
        }
    }
}