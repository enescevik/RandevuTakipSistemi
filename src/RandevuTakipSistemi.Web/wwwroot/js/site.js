function formatDateTime(date, addHours = 0, addMinutes = 0) {
    const d = new Date(date);
    d.setHours(d.getHours() + addHours);
    d.setMinutes(d.getMinutes() + addMinutes);
    return d.toISOString().slice(0, 16); // YYYY-MM-DDTHH:mm
}

FullCalendar.globalLocales.push({
    code: 'tr',
    week: {
        dow: 1,
        doy: 4,
    },
    buttonText: {
        prev: 'Geri',
        next: 'İleri',
        today: 'Bugün',
        month: 'Ay',
        week: 'Hafta',
        day: 'Gün',
        list: 'Liste',
    },
    weekText: 'Hf',
    allDayText: 'Tüm gün',
    moreLinkText: 'daha fazla',
    noEventsText: 'Gösterilecek etkinlik yok',
});