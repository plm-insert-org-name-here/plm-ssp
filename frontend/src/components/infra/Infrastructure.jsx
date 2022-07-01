import axios from "axios";
import React, { useEffect, useState } from "react";

import { Box, Paper, Tab, Tabs } from "@mui/material";

import useMounted from "../../common/hooks/useMounted";
import { Routes } from "../../routes";
import Detectors from "./Detectors";
import Locations from "./Locations";

function TabPanel({ children, value, index, ...other }) {
    return (
        <Box flex={1} display={value === index ? "flex" : "none"} flexDirection="column" {...other}>
            {children}
        </Box>
    );
}

const Infrastructure = ({ selectedLocation, setSelectedLocation }) => {
    const [selectedTab, setSelectedTab] = useState(0);

    const [detectors, setDetectors] = useState([]);
    const [locations, setLocations] = useState([]);
    const isMounted = useMounted();

    const onDetach = (detectorId) => {
        axios.post(`${Routes.detectors}/${detectorId}/detach`).then((_) => {
            const newLocations = [...locations];
            let editedLocation = newLocations.find((l) => l.detector?.id === detectorId);
            editedLocation.detector = null;

            const newDetectors = [...detectors];
            let editedDetector = newDetectors.find((d) => d.id === detectorId);
            editedDetector.location = null;

            setLocations(newLocations);
            setDetectors(newDetectors);
        });
    };

    const onAttach = (location, detector) => {
        const data = { locationId: location.id };
        axios.post(`${Routes.detectors}/${detector.id}/attach`, data).then((_) => {
            const newLocations = [...locations];
            let editedLocation = newLocations.find((l) => l.id === location.id);
            editedLocation.detector = detector;

            const newDetectors = [...detectors];
            let editedDetector = newDetectors.find((d) => d.id === detector.id);
            editedDetector.location = location;

            setLocations(newLocations);
            setDetectors(newDetectors);
        });
    };

    useEffect(() => {
        const locationsPromise = axios(Routes.locations);
        const detectorsPromise = axios(Routes.detectors);

        Promise.all([locationsPromise, detectorsPromise]).then(([locationsRes, detectorsRes]) => {
            if (isMounted()) {
                setLocations(locationsRes.data);
                setDetectors(detectorsRes.data);
            }
        });
    }, []);

    return (
        <Paper elevation={8} sx={{ display: "flex", flexDirection: "column", m: 2, flexGrow: 1 }}>
            <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
                <Tabs value={selectedTab} onChange={(_, v) => setSelectedTab(v)}>
                    <Tab label="Locations" />
                    <Tab label="Detectors" />
                </Tabs>
            </Box>
            <TabPanel value={selectedTab} index={0}>
                <Locations
                    locations={locations}
                    setLocations={setLocations}
                    selectedLocation={selectedLocation}
                    setSelectedLocation={setSelectedLocation}
                    onAttach={onAttach}
                    onDetach={onDetach}
                />
            </TabPanel>
            <TabPanel value={selectedTab} index={1}>
                <Detectors
                    detectors={detectors}
                    setDetectors={setDetectors}
                    onAttach={onAttach}
                    onDetach={onDetach}
                />
            </TabPanel>
        </Paper>
    );
};
export default Infrastructure;
