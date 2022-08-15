import axios from "axios";
import React from "react";

import { Box } from "@mui/material";

import { Routes } from "../../routes";
import DetectorCard from "./DetectorCard";

const Detectors = ({ detectors, setDetectors, onAttach, onDetach }) => {
    const onRename = (id, newName) => {
        const data = { name: newName };
        axios.put(`${Routes.detectors}/${id}`, data).then((_) => {
            const newDetectors = [...detectors];
            let editedDetector = newDetectors.find((d) => d.id === id);
            editedDetector.name = newName;

            setDetectors(newDetectors);
        });
    };

    const onDelete = (id) => {
        axios.delete(`${Routes.detectors}/${id}`).then((_) => {
            setDetectors((ds) => ds.filter((d) => d.id !== id));
        });
    };

    return (
        <Box sx={{ flexGrow: 1, height: 0 }}>
            <Box
                sx={{
                    height: "100%",
                    bgcolor: "background.panel",
                    overflowY: "auto",
                    borderRadius: "8px",
                }}
            >
                {detectors.map((d) => (
                    <DetectorCard
                        key={d.id}
                        detector={d}
                        onAttach={onAttach}
                        onDetach={onDetach}
                        onDelete={onDelete}
                        onRename={onRename}
                    />
                ))}
            </Box>
        </Box>
    );
};

export default Detectors;
