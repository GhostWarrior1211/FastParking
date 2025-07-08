using ClosedXML.Excel;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using QRCoder; // Add this using directive for QR code generation
using System.Drawing; // Needed for Bitmap

public class MainForm : Form
{
    private Button btnIngresar;
    private Button btnRetirar;
    private Button btnExportar;
    private TextBox txtPlaca;
    private ComboBox cmbTipo;
    private ListBox lstPlacas;
    private Button btnCerrar;
    private Label lblVehiculosActivos;
    private Label lblTotalRecaudado;
    private Label lblPromedioPermanencia;
    private Panel panelEstadisticas;
    private ListBox lstEspacios;
    private Panel panelGrillaEspacios;
    private Label[] lblEspaciosArray;
    // No PictureBox pbQRCode in MainForm as it's in the new form

    private Estacionamiento sistema;
    private string[] espacios = new string[]
    {
        "A1", "A2", "A3", "A4", "A5",
        "B1", "B2", "B3", "B4", "B5",
        "C1", "C2", "C3", "C4", "C5"
    };
    private Dictionary<string, string> espaciosOcupados = new Dictionary<string, string>();
    // No qrCodeImages dictionary in MainForm as images are passed directly now

    public MainForm()
    {
        this.Text = "FAST PARKING";
        this.Width = 750;
        this.Height = 520;

        sistema = new Estacionamiento(new TarifaPorTipoVehiculo(), new HistorialSalida());

        Label lblPlaca = new Label() { Text = "Placa:", Top = 20, Left = 40, Width = 50 };
        txtPlaca = new TextBox()
        {
            Top = 20,
            Left = 100,
            Width = 200,
            BackColor = System.Drawing.Color.White,
            ForeColor = System.Drawing.Color.Black
        };
        txtPlaca.MaxLength = 6;

        Label lblTipo = new Label() { Text = "Tipo:", Top = 60, Left = 40, Width = 50 };
        cmbTipo = new ComboBox()
        {
            Top = 60,
            Left = 100,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = System.Drawing.Color.White,
            ForeColor = System.Drawing.Color.Black
        };
        cmbTipo.Items.AddRange(Enum.GetNames(typeof(TipoVehiculo)));
        cmbTipo.SelectedIndex = 0;

        btnIngresar = new Button() { Text = "Ingresar", Top = 100, Left = 20 };
        btnRetirar = new Button() { Text = "Retirar", Top = 100, Left = 120 };
        btnExportar = new Button() { Text = "Exportar Historial", Top = 475, Left = 400, Width = 180 };
        btnCerrar = new Button()
        {
            Text = "Cerrar",
            Top = 475,
            Left = 600,
            Width = 100,
            Height = 35
        };
        btnCerrar.Click += BtnCerrar_Click;
        Controls.Add(btnCerrar);


        btnIngresar.Click += BtnIngresar_Click;
        btnRetirar.Click += BtnRetirar_Click;
        btnExportar.Click += BtnExportar_Click;

        lstPlacas = new ListBox() { Top = 140, Left = 20, Width = 340, Height = 100 };

        Label lblEspacios = new Label() { Text = "Espacios ocupados:", Top = 250, Left = 20, Width = 120 };
        lstEspacios = new ListBox() { Top = 275, Left = 20, Width = 340, Height = 80 };

        CrearGrillaEspacios();

        panelEstadisticas = new Panel()
        {
            Top = 20,
            Left = 400,
            Width = 300,
            Height = 200,
            BackColor = System.Drawing.Color.LightSteelBlue,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblVehiculosActivos = new Label() { Top = 20, Left = 10, Width = 280, Text = "Vehículos activos: 0", Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
        lblTotalRecaudado = new Label() { Top = 60, Left = 10, Width = 280, Text = "Total recaudado: S/ 0.00", Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
        lblPromedioPermanencia = new Label() { Top = 100, Left = 10, Width = 280, Text = "Promedio permanencia: 0 min", Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };

        Label lblEspaciosDisponibles = new Label() { Top = 140, Left = 10, Width = 280, Text = $"Espacios disponibles: {espacios.Length}", Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };

        panelEstadisticas.Controls.Add(lblVehiculosActivos);
        panelEstadisticas.Controls.Add(lblTotalRecaudado);
        panelEstadisticas.Controls.Add(lblPromedioPermanencia);
        panelEstadisticas.Controls.Add(lblEspaciosDisponibles);

        Panel panelInfo = new Panel()
        {
            Top = 240,
            Left = 400,
            Width = 300,
            Height = 220,
            BackColor = System.Drawing.Color.LightBlue,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblTituloInfo = new Label()
        {
            Text = "INFORMACIÓN DEL SISTEMA",
            Top = 10,
            Left = 10,
            Width = 280,
            Height = 25,
            Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            BackColor = System.Drawing.Color.DarkBlue,
            ForeColor = System.Drawing.Color.White
        };

        panelInfo.Controls.Add(lblTituloInfo);
        panelInfo.Controls.Add(new Label() { Text = "Capacidad Total: 15 espacios", Top = 45, Left = 10, Width = 280, Height = 20, Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular) });
        panelInfo.Controls.Add(new Label() { Text = "Horario: 24/7", Top = 70, Left = 10, Width = 280, Height = 20, Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular) });
        panelInfo.Controls.Add(new Label() { Text = "TARIFAS:", Top = 100, Left = 10, Width = 280, Height = 20, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) });
        panelInfo.Controls.Add(new Label() { Text = "• Auto: S/ 2.00/hora", Top = 125, Left = 10, Width = 280, Height = 20, Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular) });
        panelInfo.Controls.Add(new Label() { Text = "• Moto: S/ 1.00/hora", Top = 150, Left = 10, Width = 280, Height = 20, Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular) });
        panelInfo.Controls.Add(new Label() { Text = "• Camión: S/ 3.00/hora", Top = 175, Left = 10, Width = 280, Height = 20, Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular) });


        Controls.Add(lblPlaca);
        Controls.Add(txtPlaca);
        Controls.Add(lblTipo);
        Controls.Add(cmbTipo);
        Controls.Add(btnIngresar);
        Controls.Add(btnRetirar);
        Controls.Add(btnExportar);
        Controls.Add(lstPlacas);
        Controls.Add(lblEspacios);
        Controls.Add(lstEspacios);
        Controls.Add(panelGrillaEspacios);
        Controls.Add(panelEstadisticas);
        Controls.Add(panelInfo);

        ActualizarEstadisticas();
    }

    private void ActualizarEstadisticas()
    {
        int vehiculosActivos = sistema.ObtenerPlacasActuales().Count();
        int espaciosDisponibles = espacios.Length - espaciosOcupados.Count;

        lblVehiculosActivos.Text = $"Vehículos activos: {vehiculosActivos} / {espacios.Length}";

        Label lblEspaciosDisponibles = panelEstadisticas.Controls.OfType<Label>()
                                          .FirstOrDefault(l => l.Text.StartsWith("Espacios disponibles:"));
        if (lblEspaciosDisponibles != null)
        {
            lblEspaciosDisponibles.Text = $"Espacios disponibles: {espaciosDisponibles}";
        }

        var historial = sistema.ObtenerHistorial();
        decimal total = historial.Sum(h => h.Monto);
        lblTotalRecaudado.Text = $"Total recaudado: S/ {total:F2}";

        double promedio = historial.Any() ? historial.Average(h => h.Tiempo.TotalMinutes) : 0;
        lblPromedioPermanencia.Text = $"Promedio permanencia: {promedio:F1} min";

        ActualizarGrillaEspacios();
    }

    private void BtnIngresar_Click(object sender, EventArgs e)
    {
        string placa = txtPlaca.Text.Trim().ToUpper();
        if (!System.Text.RegularExpressions.Regex.IsMatch(placa, @"^[A-Z0-9]{6}$"))
        {
            MessageBox.Show("La placa debe tener exactamente 6 letras o números (sin espacios).");
            return;
        }

        if (sistema.ObtenerPlacasActuales().Contains(placa))
        {
            MessageBox.Show("La placa ya fue registrada.");
            return;
        }

        if (espaciosOcupados.Count >= espacios.Length)
        {
            MessageBox.Show("El estacionamiento está lleno. No hay espacios disponibles.");
            return;
        }

        string espacioAsignado = espacios.FirstOrDefault(f => !espaciosOcupados.ContainsKey(f));

        if (espacioAsignado == null)
        {
            MessageBox.Show("El estacionamiento está lleno. No hay espacios disponibles.");
            return;
        }

        TipoVehiculo tipo = (TipoVehiculo)Enum.Parse(typeof(TipoVehiculo), cmbTipo.SelectedItem.ToString());
        sistema.IngresarVehiculo(new Vehiculo(placa, tipo, espacioAsignado));

        espaciosOcupados[espacioAsignado] = placa;

        lstPlacas.Items.Add($"{placa} - {espacioAsignado}");
        lstEspacios.Items.Add($"{espacioAsignado}: {placa}");

        // --- IMPORTANT CHANGE HERE ---
        // Generate QR code and display it in a new pop-up form, using QRVentana
        GenerateAndShowQRVentana(placa);
        // --- END IMPORTANT CHANGE ---

        txtPlaca.Clear();
        ActualizarEstadisticas();
    }

    private void BtnRetirar_Click(object sender, EventArgs e)
    {
        if (lstPlacas.SelectedItem == null) return;

        string placaConEspacio = lstPlacas.SelectedItem.ToString();
        string placa = placaConEspacio.Split(' ')[0];

        string espacioLiberado = espaciosOcupados.FirstOrDefault(x => x.Value == placa).Key;

        TimeSpan duracion;
        decimal monto = sistema.RetirarVehiculo(placa, out duracion);

        var registro = sistema.ObtenerHistorial().LastOrDefault(r => r.Placa == placa);

        if (registro != null)
        {
            string nombreArchivo = $"recibo_{placa}.txt";
            string recibo = $"********** RECIBO FAST PARKING **********\r\n" +
                            $"Placa: {registro.Placa}\r\n" +
                            $"Tipo: {registro.Tipo}\r\n" +
                            $"Espacio: {espacioLiberado}\r\n" +
                            $"Hora de ingreso: {registro.HoraIngreso:dd/MM/yyyy HH:mm}\r\n" +
                            $"Hora de salida: {registro.HoraSalida:dd/MM/yyyy HH:mm}\r\n" +
                            $"Duración: {registro.Tiempo.TotalMinutes:F1} minutos\r\n" +
                            $"Monto: S/.{registro.Monto}\r\n" +
                            $"*****************************************";
            System.IO.File.WriteAllText(nombreArchivo, recibo);
        }

        MessageBox.Show($"Tiempo: {duracion.TotalMinutes:F1} minutos. Monto: S/.{monto}");

        if (espacioLiberado != null)
        {
            espaciosOcupados.Remove(espacioLiberado);
            lstEspacios.Items.Remove($"{espacioLiberado}: {placa}");
        }

        lstPlacas.Items.Remove(placaConEspacio);

        ActualizarEstadisticas();
    }

    private void BtnExportar_Click(object sender, EventArgs e)
    {
        var historial = sistema.ObtenerHistorial().ToList();

        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("Historial");

            ws.Cell(1, 1).Value = "Cantidad";
            ws.Cell(1, 2).Value = "Placa";
            ws.Cell(1, 3).Value = "Tipo";
            ws.Cell(1, 4).Value = "Duración";
            ws.Cell(1, 5).Value = "Hora Ingreso";
            ws.Cell(1, 6).Value = "Hora Salida";
            ws.Cell(1, 7).Value = "Monto S/";

            int row = 2;
            int contador = 1;
            foreach (var r in historial)
            {
                ws.Cell(row, 1).Value = contador;
                ws.Cell(row, 2).Value = r.Placa;
                ws.Cell(row, 3).Value = r.Tipo.ToString();
                ws.Cell(row, 4).Value = r.Tiempo.TotalMinutes.ToString("F2");
                ws.Cell(row, 5).Value = r.HoraIngreso.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 6).Value = r.HoraSalida.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 7).Value = r.Monto;
                row++;
                contador++;
            }

            ws.Cell(row, 6).Value = "Total";
            ws.Cell(row, 7).FormulaA1 = $"SUM(G2:G{row - 1})";

            workbook.SaveAs("historial_estacionamiento.xlsx");
        }

        MessageBox.Show("Historial exportado exitosamente.");
    }
    private void BtnCerrar_Click(object sender, EventArgs e)
    {
        var historial = sistema.ObtenerHistorial();
        int cantidad = historial.Count();
        decimal total = historial.Sum(r => r.Monto);

        MessageBox.Show($"Se retiraron {cantidad} vehículos.\nTotal ganado: S/.{total:F2}", "Resumen final");

        Application.Exit();
    }
    private void CrearGrillaEspacios()
    {
        panelGrillaEspacios = new Panel()
        {
            Top = 365,
            Left = 20,
            Width = 340,
            Height = 90,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = System.Drawing.Color.LightGray
        };

        Label lblTitulo = new Label()
        {
            Text = "Estado de Espacios (Verde: Disponible, Rojo: Ocupado)",
            Top = 5,
            Left = 10,
            Width = 320,
            Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        panelGrillaEspacios.Controls.Add(lblTitulo);

        lblEspaciosArray = new Label[espacios.Length];

        for (int i = 0; i < espacios.Length; i++)
        {
            int fila = i / 5;
            int columna = i % 5;

            lblEspaciosArray[i] = new Label()
            {
                Text = espacios[i],
                Top = 25 + (fila * 22),
                Left = 10 + (columna * 65),
                Width = 60,
                Height = 20,
                BackColor = System.Drawing.Color.LightGreen,
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };

            panelGrillaEspacios.Controls.Add(lblEspaciosArray[i]);
        }
    }

    private void ActualizarGrillaEspacios()
    {
        for (int i = 0; i < espacios.Length; i++)
        {
            string espacio = espacios[i];
            if (espaciosOcupados.ContainsKey(espacio))
            {
                lblEspaciosArray[i].BackColor = System.Drawing.Color.Red;
                lblEspaciosArray[i].ForeColor = System.Drawing.Color.White;
                lblEspaciosArray[i].Text = $"{espacio}\n{espaciosOcupados[espacio]}";
            }
            else
            {
                lblEspaciosArray[i].BackColor = System.Drawing.Color.LightGreen;
                lblEspaciosArray[i].ForeColor = System.Drawing.Color.Black;
                lblEspaciosArray[i].Text = espacio;
            }
        }
    }

    /// <summary>
    /// Generates a QR code for the given plate and displays it in a new pop-up form (QRVentana).
    /// </summary>
    /// <param name="placa">The vehicle's plate number.</param>
    private void GenerateAndShowQRVentana(string placa)
    {
        try
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(placa, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(8); // Increased size slightly for better pop-up display

            // Create and show the new QR code form (QRVentana)
            using (QRVentana qrForm = new QRVentana(qrCodeImage))
            {
                qrForm.ShowDialog(this); // ShowDialog makes it modal, preventing interaction with main form until closed
            }
            // The qrCodeImage Bitmap will be disposed by QRVentana's OnFormClosed event
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generando código QR: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}