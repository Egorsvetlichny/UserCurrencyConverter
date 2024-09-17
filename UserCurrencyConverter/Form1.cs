using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace UserCurrencyConverter
{
    public partial class Form1 : Form
    {
        private readonly string _apiKey = SecretFileHandler.GetSecretData("apiKey");
        private const string _apiUrl = "https://v6.exchangerate-api.com/v6/{0}/latest/{1}";

        private readonly Label _mainLabel;
        private readonly Label _fromCurrencyLabel;
        private readonly Label _toCurrencyLabel;
        private readonly Label _amountLabel;
        private readonly Label _resultLabel;
        private readonly ComboBox _fromCurrencyComboBox;
        private readonly ComboBox _toCurrencyComboBox;
        private readonly TextBox _amountTextBox;
        private readonly Button _convertButton;

        public Form1()
        {
            this.Text = "Simple currency converter";
            this.Size = new System.Drawing.Size(500, 400);
            this.BackColor = Color.LightGray;
            this.Icon = new Icon(Path.Combine(Environment.CurrentDirectory.Replace("\\bin\\Debug", "\\media"), "icon.ico"));

            _mainLabel = new Label
            {
                Text = "Выберите валюты для конвертации",
                Size = new System.Drawing.Size(500, 40),
                Font = new Font("Arial Black", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 10, 0, 0)
            };

            _fromCurrencyLabel = new Label
            {
                Text = "Исходная валюта:",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                Location = new System.Drawing.Point(55, 70)
            };

            _fromCurrencyComboBox = new ComboBox();
            _fromCurrencyComboBox.Items.AddRange(new string[] { "USD", "EUR", "RUB", "GBP", "UAH", "KZT", "CNY", "JPY", "INR" });
            _fromCurrencyComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            _fromCurrencyComboBox.Location = new System.Drawing.Point(65, 110);
            _fromCurrencyComboBox.SelectedIndex = 0;

            _toCurrencyLabel = new Label
            {
                Text = "Целевая валюта:",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                Location = new System.Drawing.Point(300, 70)
            };

            _toCurrencyComboBox = new ComboBox();
            _toCurrencyComboBox.Items.AddRange(new string[] { "USD", "EUR", "RUB", "GBP", "UAH", "KZT", "CNY", "JPY", "INR" });
            _toCurrencyComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            _toCurrencyComboBox.Location = new System.Drawing.Point(305, 110);
            _toCurrencyComboBox.SelectedIndex = 0;

            _amountLabel = new Label
            {
                Text = "Необходимая сумма:",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                Location = new System.Drawing.Point(55, 180)
            };

            _amountTextBox = new TextBox
            {
                Width = 195,
                Location = new System.Drawing.Point(230, 180)
            };

            _convertButton = new Button
            {
                Text = "Конвертировать",
                Size = new System.Drawing.Size(150, 30),
                Location = new System.Drawing.Point(160, 240),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightSlateGray
            };
            _convertButton.FlatAppearance.MouseOverBackColor = Color.LightGreen;
            _convertButton.FlatAppearance.MouseDownBackColor = Color.Green;
            _convertButton.Click += ConvertButton_Click;

            _resultLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                Location = new System.Drawing.Point(20, 290)
            };

            this.Controls.Add(_mainLabel);
            this.Controls.Add(_fromCurrencyLabel);
            this.Controls.Add(_fromCurrencyComboBox);
            this.Controls.Add(_toCurrencyLabel);
            this.Controls.Add(_toCurrencyComboBox);
            this.Controls.Add(_amountLabel);
            this.Controls.Add(_amountTextBox);
            this.Controls.Add(_convertButton);
            this.Controls.Add(_resultLabel);
        }

        private async void ConvertButton_Click(object sender, EventArgs e)
        {
            try
            {
                string fromCurrency = _fromCurrencyComboBox.Text.ToString().ToUpper();
                string toCurrency = _toCurrencyComboBox.Text.ToString().ToUpper();

                //Валидация суммы, чтобы ввод работал с точкой в качестве разделителя
                string text = _amountTextBox.Text;
                int index = text.IndexOf('.');
                if (index != -1)
                    _amountTextBox.Text = text.Substring(0, index) + "," + text.Substring(index + 1);
                else if (text == "")
                    _resultLabel.Text = $"Сначала введите сумму конвертации!";
                else
                {
                    decimal amount = decimal.Parse(_amountTextBox.Text);

                    var response = await GetExchangeRate(fromCurrency, toCurrency);
                    var exchangeRate = response.Conversion_rates[toCurrency];

                    decimal convertedAmount = amount * exchangeRate;
                    if (amount > 1_000_000m)
                    {
                        _resultLabel.Text = $"Вы можете конвертировать не более 1 000 000 валюты!";
                    }
                    else if (amount < 0m)
                    {
                        _resultLabel.Text = $"Количество валюты должно быть положительным числом!";
                    }
                    else
                    {
                        amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
                        convertedAmount = Math.Round(convertedAmount, 2, MidpointRounding.AwayFromZero);
                        _resultLabel.Text = $"Результат конвертации:\n{amount} {fromCurrency} = {convertedAmount} {toCurrency}";
                        DbHistoryWriter.SaveConversionHistory(fromCurrency, toCurrency, amount, convertedAmount);
                    }
                }

            }
            catch (HttpRequestException)
            {
                MessageBox.Show($"Ошибка при обращении к сервису ExchangeRate.\nПроверьте корректность исходной валюты", "Ошибка конвертации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show($"Ошибка: Целевая валюта указана неверно", "Ошибка конвертации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка конвертации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<ExchangeRateResponse> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("apikey", ApiKey);
                var url = string.Format(_apiUrl, _apiKey, fromCurrency);
                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ExchangeRateResponse>(content);
            }
        }

        private class ExchangeRateResponse
        {
            public string Base_code { get; set; }
            public Dictionary<string, decimal> Conversion_rates { get; set; }
        }
    }
}
