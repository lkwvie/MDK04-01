using ClassroomBooking.Configuration;
using ClassroomBooking.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ClassroomBooking.Models;

namespace ClassroomBooking
{
    public partial class MainWindow : Window
    {
        private readonly UserService userService = new UserService();
        private readonly RoomService roomService = new RoomService();
        private readonly BookingService bookingService = new BookingService();

        private readonly ConfigService configService = new ConfigService();
        private AppSettings settings;

        public MainWindow()
        {
            InitializeComponent();


            settings = configService.Load();


            Title = settings.AppTitle;
            StatusTextBlock.Text = settings.StatusMessage;

            UserComboBox.ItemsSource = userService.GetUsers();
            RoomComboBox.ItemsSource = roomService.GetRooms();
            BookingDatePicker.SelectedDate = DateTime.Today;

            RefreshBookings();
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (!(UserComboBox.SelectedItem is User user))
            {
                StatusTextBlock.Text = "Выберите пользователя.";
                return;
            }

            if (!(RoomComboBox.SelectedItem is Room room))
            {
                StatusTextBlock.Text = "Выберите аудиторию.";
                return;
            }

            
            if (!(BookingDatePicker.SelectedDate is DateTime date))
            {
                StatusTextBlock.Text = "Выберите дату.";
                return;
            }

            
            if (!TimeSpan.TryParse(StartTimeTextBox.Text, out TimeSpan startTime) ||
                !TimeSpan.TryParse(EndTimeTextBox.Text, out TimeSpan endTime))
            {
                StatusTextBlock.Text = "Некорректный формат времени (используйте ЧЧ:ММ).";
                return;
            }

            if (endTime <= startTime)
            {
                StatusTextBlock.Text = "Время окончания должно быть больше времени начала.";
                return;
            }

           
            if (!TimeSpan.TryParse(settings.WorkDayStart, out TimeSpan workStart) ||
                !TimeSpan.TryParse(settings.WorkDayEnd, out TimeSpan workEnd))
            {
                StatusTextBlock.Text = "Ошибка конфигурации рабочего времени в файле настроек.";
                return;
            }

            if (startTime < workStart || endTime > workEnd)
            {
                StatusTextBlock.Text = $"Бронирование доступно только в рабочие часы: с {settings.WorkDayStart} до {settings.WorkDayEnd}.";
                return;
            }

            
            double durationMinutes = (endTime - startTime).TotalMinutes;
            if (durationMinutes < settings.MinBookingMinutes)
            {
                StatusTextBlock.Text = $"Минимальное время бронирования — {settings.MinBookingMinutes} минут.";
                return;
            }

            
            if (!room.IsAvailable && !settings.AllowUnavailableRooms)
            {
                StatusTextBlock.Text = $"Аудитория {room.Number} сейчас недоступна для бронирования.";
                return;
            }

            
            var booking = new Booking
            {
                UserId = user.Id,
                RoomId = room.Id,
                Date = date,
                StartTime = startTime,
                EndTime = endTime
            };

            if (!bookingService.AddBooking(booking))
            {
                StatusTextBlock.Text = "На это время аудитория уже забронирована.";
                return;
            }

            StatusTextBlock.Text = "Бронирование успешно создано.";
            RefreshBookings();
        }

        private void RefreshBookings()
        {
            var rooms = roomService.GetRooms();
            var users = userService.GetUsers();

            var items = bookingService.GetBookings()
                .Select(x =>
                {
                    var u = users.FirstOrDefault(user => user.Id == x.UserId);
                    var r = rooms.FirstOrDefault(room => room.Id == x.RoomId);
                    return $"{x.Date:dd.MM.yyyy} | {x.StartTime:hh\\:mm}-{x.EndTime:hh\\:mm} | Ауд. {r?.Number} (Макс. групп: {r?.MaxGroupSize} чел.) | {u?.Name}";
                })
                .ToList();

            BookingsListBox.ItemsSource = items;
        }
    }
}
