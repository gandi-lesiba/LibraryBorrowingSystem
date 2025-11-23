using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem
{
    public class BookBorrowingForm : Form
    {
        // UI Components
        private DataGridView booksGrid, membersGrid;
        private TextBox bookSearchField, memberSearchField;
        private TextBox selectedBookId, selectedBookTitle, selectedMemberId, selectedMemberName;
        private DateTimePicker borrowDatePicker, dueDatePicker;
        private Button processBorrowButton, returnsButton;

        // Data storage
        private DataTable booksCollection, membersCollection;

        public BookBorrowingForm()
        {
            ConfigureWindow();
            BuildUserInterface();
            RegisterEventHandlers();
        }

        private void ConfigureWindow()
        {
            this.Text = "Library Book Borrowing System";
            this.Size = new Size(1020, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void BuildUserInterface()
        {
            CreateBooksSection();
            CreateMembersSection();
            CreateTransactionPanel();
        }

        private void CreateBooksSection()
        {
            var booksHeader = new Label
            {
                Text = "Available Books",
                Location = new Point(12, 10),
                AutoSize = true
            };
            this.Controls.Add(booksHeader);

            bookSearchField = new TextBox
            {
                Location = new Point(12, 32),
                Size = new Size(460, 23),
                Text = "Search by title or author...",
                ForeColor = SystemColors.GrayText
            };
            this.Controls.Add(bookSearchField);

            booksGrid = new DataGridView
            {
                Location = new Point(12, 64),
                Size = new Size(620, 320),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(booksGrid);
        }

        private void CreateMembersSection()
        {
            var membersHeader = new Label
            {
                Text = "Registered Members",
                Location = new Point(644, 10),
                AutoSize = true
            };
            this.Controls.Add(membersHeader);

            memberSearchField = new TextBox
            {
                Location = new Point(644, 32),
                Size = new Size(260, 23),
                Text = "Search by name or email...",
                ForeColor = SystemColors.GrayText
            };
            this.Controls.Add(memberSearchField);

            returnsButton = new Button
            {
                Text = "Returns",
                Location = new Point(914, 30),
                Size = new Size(80, 26)
            };
            this.Controls.Add(returnsButton);

            membersGrid = new DataGridView
            {
                Location = new Point(644, 64),
                Size = new Size(350, 320),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(membersGrid);
        }

        private void CreateTransactionPanel()
        {
            var detailsPanel = new GroupBox
            {
                Text = "Book Borrowing Details",
                Location = new Point(12, 394),
                Size = new Size(980, 270)
            };
            this.Controls.Add(detailsPanel);

            int leftColumn = 20;
            int rightColumn = 520;
            int startTop = 30;
            int rowSpacing = 36;

            // Book information
            CreateDetailField(detailsPanel, "Book ID:", leftColumn, startTop, out selectedBookId, 160, true);
            CreateDetailField(detailsPanel, "Book Title:", leftColumn, startTop + rowSpacing, out selectedBookTitle, 400, true);

            // Member information
            CreateDetailField(detailsPanel, "Member ID:", rightColumn, startTop, out selectedMemberId, 160, true);
            CreateDetailField(detailsPanel, "Member Name:", rightColumn, startTop + rowSpacing, out selectedMemberName, 360, true);

            // Date selection
            var borrowDateLabel = new Label
            {
                Text = "Borrow Date:",
                Location = new Point(leftColumn, startTop + rowSpacing * 2),
                AutoSize = true
            };
            detailsPanel.Controls.Add(borrowDateLabel);

            borrowDatePicker = new DateTimePicker
            {
                Location = new Point(leftColumn + 80, startTop + rowSpacing * 2 - 3),
                Format = DateTimePickerFormat.Short
            };
            detailsPanel.Controls.Add(borrowDatePicker);

            var dueDateLabel = new Label
            {
                Text = "Due Date:",
                Location = new Point(leftColumn + 240, startTop + rowSpacing * 2),
                AutoSize = true
            };
            detailsPanel.Controls.Add(dueDateLabel);

            dueDatePicker = new DateTimePicker
            {
                Location = new Point(leftColumn + 300, startTop + rowSpacing * 2 - 3),
                Format = DateTimePickerFormat.Short
            };
            detailsPanel.Controls.Add(dueDatePicker);

            processBorrowButton = new Button
            {
                Text = "Process Borrow",
                Location = new Point(824, 204),
                Size = new Size(120, 36)
            };
            detailsPanel.Controls.Add(processBorrowButton);
        }

        private void CreateDetailField(Control parent, string labelText, int x, int y, out TextBox field, int width, bool readOnly)
        {
            var label = new Label { Text = labelText, Location = new Point(x, y), AutoSize = true };
            parent.Controls.Add(label);

            field = new TextBox
            {
                Location = new Point(x + (labelText == "Book ID:" ? 80 : 90), y - 3),
                Width = width,
                ReadOnly = readOnly
            };
            parent.Controls.Add(field);
        }

        private void RegisterEventHandlers()
        {
            this.Load += OnFormLoading;

            // Search field events
            bookSearchField.Enter += OnSearchFieldFocus;
            bookSearchField.Leave += OnSearchFieldBlur;
            bookSearchField.TextChanged += OnBookSearch;

            memberSearchField.Enter += OnSearchFieldFocus;
            memberSearchField.Leave += OnSearchFieldBlur;
            memberSearchField.TextChanged += OnMemberSearch;

            // Grid events
            booksGrid.SelectionChanged += OnBookSelectionChange;
            membersGrid.SelectionChanged += OnMemberSelectionChange;

            // Button events
            processBorrowButton.Click += OnProcessBorrow;
            returnsButton.Click += OnReturnsButtonClick;
        }

        private void OnFormLoading(object sender, EventArgs e)
        {
            LoadBooksData();
            LoadMembersData();
        }

        private void OnSearchFieldFocus(object sender, EventArgs e)
        {
            var field = (TextBox)sender;
            if (field.Text == "Search by title or author..." || field.Text == "Search by name or email...")
            {
                field.Text = "";
                field.ForeColor = SystemColors.WindowText;
            }
        }

        private void OnSearchFieldBlur(object sender, EventArgs e)
        {
            var field = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(field.Text))
            {
                field.Text = field == bookSearchField ? "Search by title or author..." : "Search by name or email...";
                field.ForeColor = SystemColors.GrayText;
            }
        }

        private void LoadBooksData()
        {
            try
            {
                booksCollection = DataService.RetrieveData(
                    "SELECT book_id, title, author, genre, available FROM books ORDER BY title");
                booksGrid.DataSource = booksCollection;
            }
            catch (Exception ex)
            {
                ShowError("Failed to load books: " + ex.Message);
            }
        }

        private void LoadMembersData()
        {
            try
            {
                membersCollection = DataService.RetrieveData(
                    "SELECT member_id, name, email FROM members ORDER BY name");
                membersGrid.DataSource = membersCollection;
            }
            catch (Exception ex)
            {
                ShowError("Failed to load members: " + ex.Message);
            }
        }

        private void OnBookSearch(object sender, EventArgs e)
        {
            if (booksCollection == null) return;

            string searchText = bookSearchField.Text;
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search by title or author...")
            {
                booksCollection.DefaultView.RowFilter = "";
                return;
            }

            string safeSearch = searchText.Replace("'", "''");
            string filterExpression = $"title LIKE '%{safeSearch}%' OR author LIKE '%{safeSearch}%'";
            booksCollection.DefaultView.RowFilter = filterExpression;
        }

        private void OnMemberSearch(object sender, EventArgs e)
        {
            if (membersCollection == null) return;

            string searchText = memberSearchField.Text;
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search by name or email...")
            {
                membersCollection.DefaultView.RowFilter = "";
                return;
            }

            string safeSearch = searchText.Replace("'", "''");
            string filterExpression = $"name LIKE '%{safeSearch}%' OR email LIKE '%{safeSearch}%'";
            membersCollection.DefaultView.RowFilter = filterExpression;
        }

        private void OnBookSelectionChange(object sender, EventArgs e)
        {
            if (booksGrid.CurrentRow == null) return;

            try
            {
                var bookIdCell = booksGrid.CurrentRow.Cells["book_id"];
                if (bookIdCell == null || bookIdCell.Value == null) return;

                selectedBookId.Text = bookIdCell.Value.ToString();
                selectedBookTitle.Text = booksGrid.CurrentRow.Cells["title"].Value?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                ShowError("Error selecting book: " + ex.Message);
            }
        }

        private void OnMemberSelectionChange(object sender, EventArgs e)
        {
            if (membersGrid.CurrentRow == null) return;

            try
            {
                selectedMemberId.Text = membersGrid.CurrentRow.Cells["member_id"].Value?.ToString() ?? "";
                selectedMemberName.Text = membersGrid.CurrentRow.Cells["name"].Value?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                ShowError("Error selecting member: " + ex.Message);
            }
        }

        private void OnReturnsButtonClick(object sender, EventArgs e)
        {
            using (var returnsForm = new BookReturnForm())
            {
                returnsForm.ShowDialog();
            }
        }

        private void OnProcessBorrow(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedBookId.Text) || string.IsNullOrWhiteSpace(selectedMemberId.Text))
            {
                ShowInfo("Please select both a book and a member to continue.");
                return;
            }

            DateTime borrowDate = borrowDatePicker.Value.Date;
            DateTime dueDate = dueDatePicker.Value.Date;

            if (dueDate < borrowDate)
            {
                ShowInfo("Due date cannot be earlier than borrow date.");
                return;
            }

            int bookIdValue = int.Parse(selectedBookId.Text);
            int memberIdValue = int.Parse(selectedMemberId.Text);

            ProcessBorrowTransaction(bookIdValue, memberIdValue, borrowDate, dueDate);
        }

        private void ProcessBorrowTransaction(int bookId, int memberId, DateTime borrowDate, DateTime dueDate)
        {
            try
            {
                // Verify book availability
                object availability = DataService.GetSingleValue(
                    "SELECT available FROM books WHERE book_id = @bookId",
                    new SqlParameter("@bookId", bookId));

                int isAvailable = availability == null || availability == DBNull.Value ? 0 : Convert.ToInt32(availability);

                if (isAvailable == 0)
                {
                    ShowInfo("Selected book is currently unavailable (already borrowed).");
                    return;
                }

                // Create borrow record
                string insertQuery =
                    "INSERT INTO borrowrecords (book_id, member_id, borrow_date, due_date) VALUES (@book, @member, @borrow, @due)";

                DataService.ExecuteUpdate(insertQuery,
                    new SqlParameter("@book", bookId),
                    new SqlParameter("@member", memberId),
                    new SqlParameter("@borrow", borrowDate),
                    new SqlParameter("@due", dueDate)
                );

                // Update book status to unavailable
                DataService.ExecuteUpdate(
                    "UPDATE books SET available = 0 WHERE book_id = @book",
                    new SqlParameter("@book", bookId)
                );

                ShowInfo("Book borrowing transaction completed successfully.");
                LoadBooksData(); // Refresh book list
            }
            catch (Exception ex)
            {
                ShowError("Error processing borrow transaction: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}