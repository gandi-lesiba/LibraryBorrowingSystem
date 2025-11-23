using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem
{
    public class BookReturnForm : Form
    {
        // UI Controls
        private DataGridView membersGrid, borrowsGrid;
        private TextBox memberSearchField;
        private TextBox memberIdField, memberNameField;
        private TextBox transactionIdField, bookIdField, bookTitleField;
        private DateTimePicker returnDatePicker;
        private Label finesLabel;
        private Button processReturnButton;

        // Data containers
        private DataTable membersData, borrowsData;

        public BookReturnForm()
        {
            ConfigureForm();
            BuildInterface();
            AttachEvents();
        }

        private void ConfigureForm()
        {
            this.Text = $"Book Returns - {Environment.UserName} - {DateTime.Today:yyyy-MM-dd}";
            this.Size = new Size(1020, 720);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void BuildInterface()
        {
            CreateMembersSection();
            CreateBorrowsSection();
            CreateReturnPanel();
        }

        private void CreateMembersSection()
        {
            var membersLabel = new Label
            {
                Text = "Library Members",
                Location = new Point(12, 10),
                AutoSize = true
            };
            this.Controls.Add(membersLabel);

            memberSearchField = new TextBox
            {
                Location = new Point(12, 32),
                Size = new Size(460, 23),
                Text = "Enter member name or email...",
                ForeColor = SystemColors.GrayText
            };
            this.Controls.Add(memberSearchField);

            membersGrid = new DataGridView
            {
                Location = new Point(12, 64),
                Size = new Size(460, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(membersGrid);
        }

        private void CreateBorrowsSection()
        {
            var borrowsLabel = new Label
            {
                Text = "Active Borrow Records",
                Location = new Point(484, 10),
                AutoSize = true
            };
            this.Controls.Add(borrowsLabel);

            borrowsGrid = new DataGridView
            {
                Location = new Point(484, 64),
                Size = new Size(520, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(borrowsGrid);
        }

        private void CreateReturnPanel()
        {
            var detailsPanel = new GroupBox
            {
                Text = "Process Book Return",
                Location = new Point(12, 384),
                Size = new Size(990, 280)
            };
            this.Controls.Add(detailsPanel);

            int leftColumn = 20;
            int rightColumn = 520;
            int startTop = 30;
            int verticalStep = 36;

            // Member information fields
            CreateDetailField(detailsPanel, "Member ID:", leftColumn, startTop, out memberIdField, 160, true);
            CreateDetailField(detailsPanel, "Member Name:", leftColumn, startTop + verticalStep, out memberNameField, 360, true);

            // Transaction information fields
            CreateDetailField(detailsPanel, "Transaction ID:", rightColumn, startTop, out transactionIdField, 140, true);
            CreateDetailField(detailsPanel, "Book ID:", rightColumn, startTop + verticalStep, out bookIdField, 140, true);

            // Book title field
            var bookTitleLabel = new Label
            {
                Text = "Book Title:",
                Location = new Point(rightColumn + 240, startTop + verticalStep),
                AutoSize = true
            };
            detailsPanel.Controls.Add(bookTitleLabel);

            bookTitleField = new TextBox
            {
                Location = new Point(rightColumn + 320, startTop + verticalStep - 3),
                Width = 300,
                ReadOnly = true
            };
            detailsPanel.Controls.Add(bookTitleField);

            // Return date field
            var returnDateLabel = new Label
            {
                Text = "Return Date:",
                Location = new Point(leftColumn, startTop + verticalStep * 2),
                AutoSize = true
            };
            detailsPanel.Controls.Add(returnDateLabel);

            returnDatePicker = new DateTimePicker
            {
                Location = new Point(leftColumn + 100, startTop + verticalStep * 2 - 3),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            detailsPanel.Controls.Add(returnDatePicker);

            // Process return button
            processReturnButton = new Button
            {
                Text = "Complete Return",
                Location = new Point(824, 204),
                Size = new Size(140, 36)
            };
            detailsPanel.Controls.Add(processReturnButton);

            // Fines display
            finesLabel = new Label
            {
                Text = "Outstanding fines: R0.00",
                Location = new Point(leftColumn, startTop + verticalStep * 3 + 10),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold)
            };
            detailsPanel.Controls.Add(finesLabel);
        }

        private void CreateDetailField(Control parent, string labelText, int x, int y, out TextBox field, int width, bool readOnly)
        {
            var label = new Label { Text = labelText, Location = new Point(x, y), AutoSize = true };
            parent.Controls.Add(label);

            field = new TextBox
            {
                Location = new Point(x + (labelText == "Member ID:" ? 100 : 80), y - 3),
                Width = width,
                ReadOnly = readOnly
            };
            parent.Controls.Add(field);
        }

        private void AttachEvents()
        {
            this.Load += OnFormLoad;

            // Search box events
            memberSearchField.Enter += OnSearchBoxEnter;
            memberSearchField.Leave += OnSearchBoxLeave;
            memberSearchField.TextChanged += OnMemberSearch;

            // Grid events
            membersGrid.SelectionChanged += OnMemberSelectionChanged;
            borrowsGrid.SelectionChanged += OnBorrowRecordSelectionChanged;

            // Button events
            processReturnButton.Click += OnProcessReturn;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            LoadMemberData();
        }

        private void OnSearchBoxEnter(object sender, EventArgs e)
        {
            if (memberSearchField.Text == "Enter member name or email...")
            {
                memberSearchField.Text = "";
                memberSearchField.ForeColor = SystemColors.WindowText;
            }
        }

        private void OnSearchBoxLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(memberSearchField.Text))
            {
                memberSearchField.Text = "Enter member name or email...";
                memberSearchField.ForeColor = SystemColors.GrayText;
            }
        }

        private void LoadMemberData()
        {
            try
            {
                membersData = DataService.RetrieveData(
                    "SELECT member_id, name, email FROM members ORDER BY name");
                membersGrid.DataSource = membersData;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Failed to load member data: " + ex.Message);
            }
        }

        private void OnMemberSearch(object sender, EventArgs e)
        {
            if (membersData == null) return;

            string searchTerm = memberSearchField.Text;
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm == "Enter member name or email...")
            {
                membersData.DefaultView.RowFilter = "";
                return;
            }

            string safeTerm = searchTerm.Replace("'", "''");
            string filterCondition = $"name LIKE '%{safeTerm}%' OR email LIKE '%{safeTerm}%'";
            membersData.DefaultView.RowFilter = filterCondition;
        }

        private void OnMemberSelectionChanged(object sender, EventArgs e)
        {
            if (membersGrid.CurrentRow == null) return;

            try
            {
                memberIdField.Text = membersGrid.CurrentRow.Cells["member_id"].Value?.ToString() ?? "";
                memberNameField.Text = membersGrid.CurrentRow.Cells["name"].Value?.ToString() ?? "";
                LoadMemberBorrowRecords();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error selecting member: " + ex.Message);
            }
        }

        private void LoadMemberBorrowRecords()
        {
            if (string.IsNullOrWhiteSpace(memberIdField.Text)) return;

            int selectedMemberId = int.Parse(memberIdField.Text);
            string query = @"
                SELECT br.transaction_id AS id, br.book_id, b.title, 
                       br.borrow_date, br.due_date, br.return_date
                FROM borrowrecords br
                JOIN books b ON br.book_id = b.book_id
                WHERE br.member_id = @memberId
                ORDER BY br.borrow_date DESC";

            try
            {
                borrowsData = DataService.RetrieveData(query,
                    new SqlParameter("@memberId", selectedMemberId));
                borrowsGrid.DataSource = borrowsData;
                ComputeOutstandingFines();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading borrow records: " + ex.Message);
            }
        }

        private void OnBorrowRecordSelectionChanged(object sender, EventArgs e)
        {
            if (borrowsGrid.CurrentRow == null) return;

            try
            {
                transactionIdField.Text = borrowsGrid.CurrentRow.Cells["id"].Value?.ToString() ?? "";
                bookIdField.Text = borrowsGrid.CurrentRow.Cells["book_id"].Value?.ToString() ?? "";
                bookTitleField.Text = borrowsGrid.CurrentRow.Cells["title"].Value?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error selecting borrow record: " + ex.Message);
            }
        }

        private void ComputeOutstandingFines()
        {
            decimal totalFines = 0m;
            if (borrowsData == null)
            {
                finesLabel.Text = "Outstanding fines: R0.00";
                return;
            }

            foreach (DataRow record in borrowsData.Rows)
            {
                if (record["return_date"] == DBNull.Value ||
                    string.IsNullOrWhiteSpace(record["return_date"].ToString()))
                {
                    if (record["due_date"] == DBNull.Value) continue;

                    DateTime dueDate = Convert.ToDateTime(record["due_date"]).Date;
                    DateTime currentDate = DateTime.Today;

                    if (dueDate < currentDate)
                    {
                        int overdueDays = (currentDate - dueDate).Days;
                        totalFines += overdueDays * 5; // R5 per overdue day
                    }
                }
            }
            finesLabel.Text = $"Outstanding fines: R{totalFines}";
        }

        private void OnProcessReturn(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(transactionIdField.Text))
            {
                ShowInformationMessage("Please select a borrow record to process return.");
                return;
            }

            int selectedTransactionId = int.Parse(transactionIdField.Text);
            int selectedBookId = int.Parse(bookIdField.Text);
            DateTime selectedReturnDate = returnDatePicker.Value.Date;

            if (IsTransactionAlreadyReturned(selectedTransactionId))
            {
                ShowInformationMessage("This book has already been returned.");
                return;
            }

            ProcessBookReturn(selectedTransactionId, selectedBookId, selectedReturnDate);
        }

        private bool IsTransactionAlreadyReturned(int transactionId)
        {
            object returnDate = DataService.GetSingleValue(
                "SELECT return_date FROM borrowrecords WHERE transaction_id = @transId",
                new SqlParameter("@transId", transactionId));

            return returnDate != null && returnDate != DBNull.Value;
        }

        private void ProcessBookReturn(int transactionId, int bookId, DateTime returnDate)
        {
            try
            {
                // Update borrow record with return date
                DataService.ExecuteUpdate(
                    "UPDATE borrowrecords SET return_date = @returnDate WHERE transaction_id = @transId",
                    new SqlParameter("@returnDate", returnDate),
                    new SqlParameter("@transId", transactionId)
                );

                // Mark book as available
                DataService.ExecuteUpdate(
                    "UPDATE books SET available = 1 WHERE book_id = @bookId",
                    new SqlParameter("@bookId", bookId)
                );

                ShowInformationMessage("Book return processed successfully.");
                LoadMemberBorrowRecords(); // Refresh the data
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error processing return: " + ex.Message);
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowInformationMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}