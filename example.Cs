log("Client script triggered: Fetching shipment status...");

const shipmentID = ZDK.Page.getField('Delivery_Number').getValue();
log("Shipment ID: " + shipmentID);
const recordId = $Page.record_id;
log("Record ID: " + recordId);

if (shipmentID) {
    const requestBody = `ARGUMENTS=-N${shipmentID}`;
    log("Request Body: " + requestBody);

    async function fetchShipmentStatus() {
        const response = await ZDK.HTTP.request({
            url: 'https://chita-il.com/RunCom.Server/Request.aspx?APPNAME=run&PRGNAME=ship_status_xml',
            method: 'POST',
            content: requestBody,
            headers: {
                'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwczovL3J1bmNvbS5jby5pbC9jbGFpbXMvY2xpZW50bm8iOiIyODQwMCIsImh0dHBzOi8vcnVuY29tLmNvLmlsL2NsYWltcy9waHJhc2UiOiI5NWRkMWQ4Ny0xMTQ4LTQ5MzctYjNiNC01MTc2Y2I2YjcyYTYiLCJleHAiOjE3NjI0MTU4ODAsImlzcyI6Imh0dHBzOi8vcnVuY29tLmNvLmlsIiwiYXVkIjoiaHR0cHM6Ly9ydW5jb20uY28uaWwifQ.UT1p2tMqq3y9Rq0FT32eTYJS-X_ZpjdhLaYOR7xAfag',
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        });

        const xmlString = await response.getResponse();
        log("Raw XML Response: " + xmlString);

        if (xmlString && typeof xmlString === "string") {
            let matches = [...xmlString.matchAll(/<([^\/>]+)>([^<]+)<\/\1>/g)];
            let values = matches.map(match => ({
                tag: match[1],
                value: match[2]
            }));

            let dataObject = {};
            values.forEach(item => {
                dataObject[item.tag] = item.value;
            });

            log("Parsed Data: " + JSON.stringify(dataObject));

            if (dataObject.status_code && dataObject.status_desc) {
                // Update fields using Zoho CRM API
              var shipment_Details = ZDK.Apps.CRM.Shipments.fetchById($Page.record_id);

                shipment_Details.Status_Code = dataObject.status_code;
                shipment_Details.Status_Description = dataObject.status_desc;
               
                var response_ = shipment_Details.update();
                              log("response_" +response_);


                log("Field update response: " + JSON.stringify(updateResponse));
            }
        }
    }

    fetchShipmentStatus();
} else {
    log("Shipment ID not found.");
}
