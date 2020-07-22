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
            //设置跨域Cores
            services.AddCors(option => option.AddPolicy("cors",
                c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddScoped<SwaggerGenerator>(); //注入SwaggerGenerator,后面可以直接使用这个方法
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",   //版本 
                    Title = $"XUnit.Core 接口文档-NetCore3.1",  //标题
                    Description = $"XUnit.Core Http API v1",    //描述
                    Contact = new OpenApiContact { Name = "艾三元", Email = "", Url = new Uri("http://i3yuan.cnblogs.com") },  
                    License = new OpenApiLicense { Name = "艾三元许可证", Url = new Uri("http://i3yuan.cnblogs.com") }
                });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
               //var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "XUnit.Core.xml");//这个就是刚刚配置的xml文件名
               // c.IncludeXmlComments(xmlPath);//默认的第二个参数是false,对方法的注释
                 c.IncludeXmlComments(xmlPath,true); // 这个是controller的注释

                #region 加锁
                var openApiSecurity = new OpenApiSecurityScheme
                {
                    Description = "JWT认证授权，使用直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",  //jwt 默认参数名称
                    In = ParameterLocation.Header,  //jwt默认存放Authorization信息的位置（请求头）
                    Type = SecuritySchemeType.ApiKey
                };

                c.AddSecurityDefinition("oauth2", openApiSecurity);
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                //在header中添加token,传递到后台
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                #endregion
            });
            services.AddScoped<SpireDocHelper>();


            var Issurer = "JWTBearer.Auth";  //发行人
            var Audience = "api.auth";       //受众人
            var secretCredentials = "q2xiARx$4x3TKqBJ";   //密钥

            //配置认证服务
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o => {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //是否验证发行人
                    ValidateIssuer = true,
                    ValidIssuer = Issurer,//发行人
                    //是否验证受众人
                    ValidateAudience = true,
                    ValidAudience = Audience,//受众人
                    //是否验证密钥
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretCredentials)),

                    ValidateLifetime = true, //验证生命周期
                    RequireExpirationTime = true, //过期时间
                };
            });
            //添加授权角色策略
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("BaseRole", options => options.RequireRole("admin"));
            //});
            //或者指定多个允许的角色
            //services.AddAuthorization(options =>
            // {
            //    options.AddPolicy("MoreBaseRole", options => options.RequireRole("admin","user"));
            // });

            //添加基于声明的授权
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("BaseClaims", options => options.RequireClaim("name"));
            //});
            ////添加基于声明的授权,指定允许值列表。
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("BaseClaims", options => options.RequireClaim("name", "i3yuan"));
            //});

            //基于自定义策略授权
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


            //此外，还需要在 IAuthorizationHandler 类型的范围内向 DI 系统注册新的处理程序：
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
            // 如前所述，要求可包含多个处理程序。如果为授权层的同一要求向 DI 系统注册多个处理程序，有一个成功就足够了。



            //var combindPolicy = new AuthorizationPolicyBuilder().RequireClaim("role").Build();
            //services.AddAuthorization(options =>
            //{

            //    //DenyAnonymousAuthorizationRequirement
            //    options.AddPolicy("DenyAnonyUser", policy => policy.RequireAuthenticatedUser());

            //    //NameAuthorizationRequirement
            //    options.AddPolicy("NameAuth", policy => policy.RequireUserName("艾三元"));

            //    //ClaimsAuthorizationRequirement
            //    options.AddPolicy("ClaimsAuth", policy => policy.RequireClaim("role", "admin"));

            //    //RolesAuthorizationRequirement
            //    options.AddPolicy("RolesAuth", policy => policy.RequireRole("admin", "user"));

            //    //AssertionRequirement
            //    options.AddPolicy("AssertAuth", policy => policy.RequireAssertion(c => c.User.HasClaim(o => o.Type == "role")));


            //    //同样可可用直接调用Combind方法，策略AuthorizationPolicy
            //    options.AddPolicy("CombindAuth", policy => policy.Combine(combindPolicy));
            //});


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                #region Swagger 只在开发环节中使用
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"XUnit.Core V1");
                    c.RoutePrefix = string.Empty;     //如果是为空 访问路径就为 根域名/index.html,注意localhost:8001/swagger是访问不到的
                                                      //路径配置，设置为空，表示直接在根域名（localhost:8001）访问该文件
                                                      // c.RoutePrefix = "swagger"; // 如果你想换一个路径，直接写名字即可，比如直接写c.RoutePrefix = "swagger"; 则访问路径为 根域名/swagger/index.html

                    c.DocumentTitle = "XUnit.Core 在线文档调试";
                    #region 自定义样式

                    //css 注入
                    c.InjectStylesheet("/css/swaggerdoc.css");
                    c.InjectStylesheet("/css/app.min.css");
                    //js 注入
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
            app.UseCors("cors");//跨域
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
