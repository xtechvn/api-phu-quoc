using Caching.RedisWorker;
using Entities.ConfigModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.IRepositories;
using Repositories.Repositories;
using REPOSITORIES.IRepositories;
using REPOSITORIES.IRepositories.Clients;
using REPOSITORIES.IRepositories.Contract;
using REPOSITORIES.IRepositories.Elasticsearch;
using REPOSITORIES.IRepositories.Fly;
using REPOSITORIES.IRepositories.Hotel;
using REPOSITORIES.IRepositories.Notify;
using REPOSITORIES.IRepositories.VinWonder;
using REPOSITORIES.Repositories;
using REPOSITORIES.Repositories.Clients;
using REPOSITORIES.Repositories.Elasticsearch;
using REPOSITORIES.Repositories.Fly;
using REPOSITORIES.Repositories.Hotel;
using REPOSITORIES.Repositories.Notify;
using REPOSITORIES.Repositories.VinWonder;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// khoi tao lan dau tien chuoi config khi ung dung duoc chay.
// no chi die khi ung ung die
// Get config to instance model
var Configuration = builder.Configuration;

builder.Services.Configure<DataBaseConfig>(Configuration.GetSection("DataBaseConfig"));


// Register services   
builder.Services.AddSingleton(Configuration);
builder.Services.AddSingleton<IServicePiceRepository, ServicePiceRepository>();
builder.Services.AddSingleton<ICampaignRepository, CampaignRepository>();
builder.Services.AddSingleton<IProductFlyTicketServiceRepository, ProductFlyTicketServiceRepository>();
builder.Services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
builder.Services.AddTransient<IClientRepository, ClientRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRoomFunRepository, RoomFunRepository>();
builder.Services.AddTransient<ITelegramRepository, TelegramRepository>();
builder.Services.AddTransient<IGroupClassAirlinesDetailRepository, GroupClassAirlinesDetailRepository>();
builder.Services.AddTransient<IGroupClassAirlinesRepository, GroupClassAirlinesRepository>();
builder.Services.AddTransient<IAirlinesRepository, AirlinesRepository>();
builder.Services.AddTransient<ITransactionRepository, TransactionRepository>();
builder.Services.AddTransient<IBankOnePayRepository, BankOnePayRepository>();
builder.Services.AddTransient<IAccountB2CRepository, AccountB2CRepository>();
builder.Services.AddTransient<IContactClientRepository, ContactClientRepository>();
builder.Services.AddTransient<IFlyBookingDetailRepository, FlyBookingDetailRepository>();
builder.Services.AddTransient<IFlightSegmentRepository, FlightSegmentRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IBagageRepository, BagageRepository>();
builder.Services.AddTransient<IPassengerRepository, PassengerRepository>();
builder.Services.AddTransient<IAirPortCodeRepository, AirPortCodeRepository>();
builder.Services.AddTransient<IArticleRepository, ArticleRepository>();
builder.Services.AddTransient<IAttachFileRepository, AttachFileRepository>();
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<IDepositHistoryRepository, DepositHistoryRepository>();
builder.Services.AddTransient<IAllotmentFundRepository, AllotmentFundRepository>();
builder.Services.AddTransient<IServicePiceRoomRepository, ServicePriceRoomRepository>();
builder.Services.AddTransient<IElasticsearchDataRepository, ElasticsearchDataRepository>();
builder.Services.AddTransient<IAccountClientRepository, AccountClientRepository>();
builder.Services.AddTransient<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<IGroupProductRepository, GroupProductRepository>();
builder.Services.AddTransient<IIdentifierServiceRepository, IdentifierServiceRepository>();
builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();
builder.Services.AddTransient<IContractPayRepository, ContractPayRepository>();
builder.Services.AddTransient<IHotelDetailRepository, HotelDetailRepository>();
builder.Services.AddTransient<IContractRepository, ContractRepository>();
builder.Services.AddTransient<IVoucherRepository, VoucherRepository>();
builder.Services.AddTransient<IAccountB2BRepository, AccountB2BRepository>();
builder.Services.AddTransient<IPlayGroundDetailRepository, PlayGroundDetailRepository>();
builder.Services.AddTransient<INotifyRepository, NotifyRepository>();

builder.Services.AddTransient<IOtherBookingRepository, OtherBookingRepository>();
builder.Services.AddTransient<ITourRepository, TourRepository>();
builder.Services.AddTransient<IAllCodeRepository, AllCodeRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IHotelBookingRepositories, HotelBookingRepositories>();

builder.Services.AddSwaggerGen();

// Mongo DB                        
builder.Services.AddSingleton<IFlyBookingMongoRepository, FlyBookingMongoRepository>();
builder.Services.AddSingleton<IHotelBookingMongoRepository, HotelBookingMongoRepository>();
builder.Services.AddSingleton<IVinWonderBookingRepository, VinWonderBookingRepository>();

builder.Services.AddControllersWithViews();


builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressConsumesConstraintForFormFileParameters = true;
        options.SuppressInferBindingSourcesForParameters = true;
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IgnoreNullValues = true;
    });

// Set session
builder.Services.AddDistributedMemoryCache();

// Setting Redis                     
builder.Services.AddSingleton<RedisConn>();

builder.Services.AddSession(option =>
{
    // Set a short timeout for easy testing.
    option.IdleTimeout = TimeSpan.FromDays(1);
    option.Cookie.HttpOnly = true;
    // Make the session cookie essential
    option.Cookie.IsEssential = true;
});


builder.Services.AddCors(o => o.AddPolicy("MyApi", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));


//Configure authorization middleware in the startup configureService method.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = Configuration["Jwt:Audience"],
        ValidIssuer = Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

}
app.UseHttpsRedirection();
//app.UseAntiXssMiddleware();
app.UseRouting();



// Inject the authorization middleware into the Request pipeline.
app.UseAuthentication();
app.UseAuthorization();

//Redis conn Call the connect method
app.UseCors("MyApi");


app.MapControllers();

app.Run();
