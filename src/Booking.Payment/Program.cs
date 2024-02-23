using Booking.Payment.Configurations;

namespace Booking.Payment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IConfiguration configuration = new ConfigurationBuilder().Build();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("http://localhost:5000")
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddSwashbuckle(configuration);

            var app = builder.Build();

            app.UseCors("AllowSpecificOrigin");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();
                app.UseSwashbuckle(configuration);
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
