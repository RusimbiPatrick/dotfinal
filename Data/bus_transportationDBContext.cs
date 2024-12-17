using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace bus_transport_mgt_sys.Models;

public partial class bus_transportationDBContext : DbContext
{
    public bus_transportationDBContext(DbContextOptions<bus_transportationDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Stop> Stops { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.PlateNo);

            entity.ToTable("bus");

            entity.Property(e => e.PlateNo)
                .HasMaxLength(50)
                .HasColumnName("plate_no");
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.NoOfSeats).HasColumnName("no_of_seats");
            entity.Property(e => e.RouteId).HasColumnName("routeId");

            entity.HasOne(d => d.Route).WithMany(p => p.Buses)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bus_route");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("drivers");

            entity.Property(e => e.DriverId)
                .ValueGeneratedOnAdd()
                .HasColumnName("driverId");
            entity.Property(e => e.AssignedBusId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("assignedBusID");
            entity.Property(e => e.Licence)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("licence");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.AssignedBus).WithMany(p => p.Drivers)
                .HasForeignKey(d => d.AssignedBusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_drivers_bus");

            entity.HasOne(d => d.DriverNavigation).WithOne(p => p.Driver)
                .HasForeignKey<Driver>(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_drivers_USERS");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payment");

            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("amount");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("paymentDate");
            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("paymentMethod");
            entity.Property(e => e.TicketId).HasColumnName("ticketId");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_payment_Ticket");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("route");

            entity.Property(e => e.RouteId).HasColumnName("routeId");
            entity.Property(e => e.Departure)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("departure");
            entity.Property(e => e.Destination)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("destination");
            entity.Property(e => e.RouteName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("routeName");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedule");

            entity.Property(e => e.ScheduleId).HasColumnName("scheduleID");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.ArrivalTime)
                .HasColumnType("datetime")
                .HasColumnName("arrivalTime");
            entity.Property(e => e.BusId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("busId");
            entity.Property(e => e.DepartureTime)
                .HasColumnType("datetime")
                .HasColumnName("departureTime");

            entity.HasOne(d => d.Bus).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedule_bus");
        });

        modelBuilder.Entity<Stop>(entity =>
        {
            entity.ToTable("stops");

            entity.Property(e => e.StopId).HasColumnName("stopId");
            entity.Property(e => e.RouteId).HasColumnName("routeId");
            entity.Property(e => e.StopName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("stopName");

            entity.HasOne(d => d.Route).WithMany(p => p.Stops)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_stops_route");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId).HasColumnName("ticketId");
            entity.Property(e => e.BusId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("busId");
            entity.Property(e => e.ScheduleId).HasColumnName("scheduleId");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Bus).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_bus");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_schedule");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_USERS");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(70)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("CLIENT")
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}