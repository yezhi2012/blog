using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using XUnit.Core.Helper;
using XUnit.Core.Policy;

namespace XUnit.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //���ÿ���Cores
            services.AddCors(option => option.AddPolicy("cors",
                c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddScoped<SwaggerGenerator>(); //ע��SwaggerGenerator,�������ֱ��ʹ���������
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",   //�汾 
                    Title = $"XUnit.Core �ӿ��ĵ�-NetCore3.1",  //����
                    Description = $"XUnit.Core Http API v1",    //����
                    Contact = new OpenApiContact { Name = "����Ԫ", Email = "", Url = new Uri("http://i3yuan.cnblogs.com") },  
                    License = new OpenApiLicense { Name = "����Ԫ���֤", Url = new Uri("http://i3yuan.cnblogs.com") }
                });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
               //var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "XUnit.Core.xml");//������Ǹո����õ�xml�ļ���
               // c.IncludeXmlComments(xmlPath);//Ĭ�ϵĵڶ���������false,�Է�����ע��
                 c.IncludeXmlComments(xmlPath,true); // �����controller��ע��

                #region ����
                var openApiSecurity = new OpenApiSecurityScheme
                {
                    Description = "JWT��֤��Ȩ��ʹ��ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
                    Name = "Authorization",  //jwt Ĭ�ϲ�������
                    In = ParameterLocation.Header,  //jwtĬ�ϴ��Authorization��Ϣ��λ�ã�����ͷ��
                    Type = SecuritySchemeType.ApiKey
                };

                c.AddSecurityDefinition("oauth2", openApiSecurity);
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                //��header�����token,���ݵ���̨
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                #endregion
            });
            services.AddScoped<SpireDocHelper>();


            var Issurer = "JWTBearer.Auth";  //������
            var Audience = "api.auth";       //������
            var secretCredentials = "q2xiARx$4x3TKqBJ";   //��Կ

            //������֤����
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o => {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //�Ƿ���֤������
                    ValidateIssuer = true,
                    ValidIssuer = Issurer,//������
                    //�Ƿ���֤������
                    ValidateAudience = true,
                    ValidAudience = Audience,//������
                    //�Ƿ���֤��Կ
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretCredentials)),

                    ValidateLifetime = true, //��֤��������
                    RequireExpirationTime = true, //����ʱ��
                };
            });
            //�����Ȩ��ɫ����
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("BaseRole", options => options.RequireRole("admin"));
            //});
            //����ָ���������Ľ�ɫ
            //services.AddAuthorization(options =>
            // {
            //    options.AddPolicy("MoreBaseRole", options => options.RequireRole("admin","user"));
            // });

            //��ӻ�����������Ȩ
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("BaseClaims", options => options.RequireClaim("name"));
            //});
            ////��ӻ�����������Ȩ,ָ������ֵ�б�
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("BaseClaims", options => options.RequireClaim("name", "i3yuan"));
            //});

            //�����Զ��������Ȩ
            services.AddAuthorization(options =>
            {
                options.AddPolicy("customizePermisson",
                  policy => policy
                    .Requirements
                    .Add(new PermissionRequirement("admin")));
            });

            //    services.AddAuthorization(options =>
            //    {

            //        options.AddPolicy("AuthUser", policy => policy.RequireAuthenticatedUser());

            //        options.AddPolicy("User", policy => policy
            //    .RequireAssertion(context => context.User.HasClaim(c => (c.Type == "EmployeeNumber" || c.Type == "Role")))
            //);
            //    });


            //���⣬����Ҫ�� IAuthorizationHandler ���͵ķ�Χ���� DI ϵͳע���µĴ������
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
            // ��ǰ������Ҫ��ɰ����������������Ϊ��Ȩ���ͬһҪ���� DI ϵͳע�������������һ���ɹ����㹻�ˡ�



            //var combindPolicy = new AuthorizationPolicyBuilder().RequireClaim("role").Build();
            //services.AddAuthorization(options =>
            //{

            //    //DenyAnonymousAuthorizationRequirement
            //    options.AddPolicy("DenyAnonyUser", policy => policy.RequireAuthenticatedUser());

            //    //NameAuthorizationRequirement
            //    options.AddPolicy("NameAuth", policy => policy.RequireUserName("����Ԫ"));

            //    //ClaimsAuthorizationRequirement
            //    options.AddPolicy("ClaimsAuth", policy => policy.RequireClaim("role", "admin"));

            //    //RolesAuthorizationRequirement
            //    options.AddPolicy("RolesAuth", policy => policy.RequireRole("admin", "user"));

            //    //AssertionRequirement
            //    options.AddPolicy("AssertAuth", policy => policy.RequireAssertion(c => c.User.HasClaim(o => o.Type == "role")));


            //    //ͬ���ɿ���ֱ�ӵ���Combind����������AuthorizationPolicy
            //    options.AddPolicy("CombindAuth", policy => policy.Combine(combindPolicy));
            //});


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                #region Swagger ֻ�ڿ���������ʹ��
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"XUnit.Core V1");
                    c.RoutePrefix = string.Empty;     //�����Ϊ�� ����·����Ϊ ������/index.html,ע��localhost:8001/swagger�Ƿ��ʲ�����
                                                      //·�����ã�����Ϊ�գ���ʾֱ���ڸ�������localhost:8001�����ʸ��ļ�
                                                      // c.RoutePrefix = "swagger"; // ������뻻һ��·����ֱ��д���ּ��ɣ�����ֱ��дc.RoutePrefix = "swagger"; �����·��Ϊ ������/swagger/index.html

                    c.DocumentTitle = "XUnit.Core �����ĵ�����";
                    #region �Զ�����ʽ

                    //css ע��
                    c.InjectStylesheet("/css/swaggerdoc.css");
                    c.InjectStylesheet("/css/app.min.css");
                    //js ע��
                    c.InjectJavascript("/js/jquery.js");
                    c.InjectJavascript("/js/swaggerdoc.js");
                    c.InjectJavascript("/js/app.min.js");
                   
                    #endregion

                });
                #endregion

            }


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("cors");//����
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
