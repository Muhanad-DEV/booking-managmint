const API_BASE_URL = window.location.origin;

function showMessage(message, type = 'info') {
    const messagesDiv = document.getElementById('messages');
    const alertClass = type === 'error' ? 'alert-danger' : type === 'success' ? 'alert-success' : 'alert-info';
    const alert = document.createElement('div');
    alert.className = `alert ${alertClass} alert-dismissible fade show`;
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    messagesDiv.appendChild(alert);
    setTimeout(() => alert.remove(), 5000);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString();
}

// Search Events
document.getElementById('searchForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const searchQuery = document.getElementById('searchQuery').value;
    const category = document.getElementById('category').value;
    
    let url = `${API_BASE_URL}/api/EventsApi?`;
    if (searchQuery) url += `searchQuery=${encodeURIComponent(searchQuery)}&`;
    if (category) url += `category=${encodeURIComponent(category)}&`;
    
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        
        const events = await response.json();
        displayEvents(events);
        showMessage(`Found ${events.length} event(s)`, 'success');
    } catch (error) {
        showMessage(`Error searching events: ${error.message}`, 'error');
        document.getElementById('eventsResult').innerHTML = '';
    }
});

async function loadAllEvents() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/EventsApi`);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        
        const events = await response.json();
        displayEvents(events);
        showMessage(`Loaded ${events.length} event(s)`, 'success');
    } catch (error) {
        showMessage(`Error loading events: ${error.message}`, 'error');
        document.getElementById('eventsResult').innerHTML = '';
    }
}

function displayEvents(events) {
    const resultDiv = document.getElementById('eventsResult');
    
    if (events.length === 0) {
        resultDiv.innerHTML = '<div class="alert alert-info">No events found.</div>';
        return;
    }
    
    let html = '<div class="row">';
    events.forEach(event => {
        html += `
            <div class="col-md-6 event-card">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">${event.title}</h5>
                        <p class="card-text"><small class="text-muted">${event.category}</small></p>
                        <p class="card-text">${event.description || 'No description'}</p>
                        <p class="card-text">
                            <strong>Date:</strong> ${formatDate(event.dateTime)}<br>
                            <strong>Venue:</strong> ${event.venue}<br>
                            <strong>Price:</strong> $${event.price.toFixed(2)}<br>
                            <strong>Available:</strong> ${event.remainingSeats} / ${event.capacity}
                        </p>
                        <p class="text-muted"><small>Event ID: ${event.eventId}</small></p>
                        <button class="btn btn-sm btn-primary" onclick="document.getElementById('eventId').value='${event.eventId}'">Use for Booking</button>
                    </div>
                </div>
            </div>
        `;
    });
    html += '</div>';
    resultDiv.innerHTML = html;
}

// Book Ticket
document.getElementById('bookTicketForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const eventId = document.getElementById('eventId').value;
    const quantity = parseInt(document.getElementById('quantity').value);
    
    if (!eventId) {
        showMessage('Please enter an Event ID', 'error');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/TicketsApi`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                eventId: eventId,
                quantity: quantity
            })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || `HTTP error! status: ${response.status}`);
        }
        
        const result = await response.json();
        document.getElementById('bookTicketResult').innerHTML = `
            <div class="alert alert-success">
                <strong>Success!</strong> ${result.message}<br>
                <small>Tickets: ${JSON.stringify(result.tickets, null, 2)}</small>
            </div>
        `;
        showMessage('Ticket booked successfully!', 'success');
        document.getElementById('bookTicketForm').reset();
    } catch (error) {
        document.getElementById('bookTicketResult').innerHTML = `
            <div class="alert alert-danger">Error: ${error.message}</div>
        `;
        showMessage(`Error booking ticket: ${error.message}`, 'error');
    }
});

// Load My Tickets
async function loadMyTickets() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/TicketsApi`);
        if (!response.ok) {
            if (response.status === 401) {
                throw new Error('Please log in to view your tickets');
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const tickets = await response.json();
        displayTickets(tickets);
        showMessage(`Loaded ${tickets.length} ticket(s)`, 'success');
    } catch (error) {
        showMessage(`Error loading tickets: ${error.message}`, 'error');
        document.getElementById('ticketsResult').innerHTML = '';
    }
}

function displayTickets(tickets) {
    const resultDiv = document.getElementById('ticketsResult');
    
    if (tickets.length === 0) {
        resultDiv.innerHTML = '<div class="alert alert-info">You don\'t have any tickets.</div>';
        return;
    }
    
    let html = '<div class="list-group">';
    tickets.forEach(ticket => {
        html += `
            <div class="list-group-item ticket-item">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <h6 class="mb-1">${ticket.eventTitle}</h6>
                        <p class="mb-1">
                            <strong>Ticket ID:</strong> ${ticket.ticketId}<br>
                            <strong>QR Code:</strong> ${ticket.qrCode}<br>
                            <strong>Status:</strong> <span class="badge bg-${getStatusColor(ticket.status)}">${ticket.status}</span><br>
                            <strong>Event Date:</strong> ${formatDate(ticket.eventDateTime)}<br>
                            <strong>Venue:</strong> ${ticket.eventVenue}<br>
                            <strong>Price:</strong> $${ticket.eventPrice.toFixed(2)}<br>
                            <strong>Purchased:</strong> ${formatDate(ticket.purchasedAt)}
                        </p>
                    </div>
                    <button class="btn btn-sm btn-danger" onclick="document.getElementById('ticketId').value='${ticket.ticketId}'">Use for Cancel</button>
                </div>
            </div>
        `;
    });
    html += '</div>';
    resultDiv.innerHTML = html;
}

function getStatusColor(status) {
    switch(status.toLowerCase()) {
        case 'paid': return 'success';
        case 'cancelled': return 'danger';
        case 'checkedin': return 'info';
        default: return 'warning';
    }
}

// Cancel Ticket
document.getElementById('cancelTicketForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const ticketId = document.getElementById('ticketId').value;
    
    if (!ticketId) {
        showMessage('Please enter a Ticket ID', 'error');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/TicketsApi/${ticketId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                status: 'cancelled'
            })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || `HTTP error! status: ${response.status}`);
        }
        
        const result = await response.json();
        document.getElementById('cancelTicketResult').innerHTML = `
            <div class="alert alert-success">
                <strong>Success!</strong> Ticket cancelled successfully.<br>
                <small>Status: ${result.status}</small>
            </div>
        `;
        showMessage('Ticket cancelled successfully!', 'success');
        document.getElementById('cancelTicketForm').reset();
        loadMyTickets(); // Refresh tickets list
    } catch (error) {
        document.getElementById('cancelTicketResult').innerHTML = `
            <div class="alert alert-danger">Error: ${error.message}</div>
        `;
        showMessage(`Error cancelling ticket: ${error.message}`, 'error');
    }
});

// Create Event
document.getElementById('createEventForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const eventData = {
        title: document.getElementById('eventTitle').value,
        description: document.getElementById('eventDescription').value,
        dateTime: document.getElementById('eventDateTime').value,
        venue: document.getElementById('eventVenue').value,
        capacity: parseInt(document.getElementById('eventCapacity').value),
        price: parseFloat(document.getElementById('eventPrice').value),
        category: document.getElementById('eventCategory').value
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/EventsApi`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(eventData)
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || `HTTP error! status: ${response.status}`);
        }
        
        const result = await response.json();
        document.getElementById('createEventResult').innerHTML = `
            <div class="alert alert-success">
                <strong>Success!</strong> Event created successfully!<br>
                <small>Event ID: ${result.eventId}</small>
            </div>
        `;
        showMessage('Event created successfully!', 'success');
        document.getElementById('createEventForm').reset();
    } catch (error) {
        document.getElementById('createEventResult').innerHTML = `
            <div class="alert alert-danger">Error: ${error.message}</div>
        `;
        showMessage(`Error creating event: ${error.message}`, 'error');
    }
});

// Set default datetime to 7 days from now
const defaultDateTime = new Date();
defaultDateTime.setDate(defaultDateTime.getDate() + 7);
document.getElementById('eventDateTime').value = defaultDateTime.toISOString().slice(0, 16);


