import React, { createContext, useContext, useState, useEffect } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';

const SignalRContext = createContext();

export const useSignalR = () => {
    return useContext(SignalRContext);
};

export const SignalRProvider = ({ children }) => {
    const [connection, setConnection] = useState(null);
    const [messages, setMessages] = useState([]);
    const [codes, setCodes] = useState([]);
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: 10, totalItems: 0 });

    useEffect(() => {
        const establishConnection = async () => {
            const conn = new HubConnectionBuilder()
                .withUrl(`${process.env.REACT_APP_SIGNALR_SERVER}`, { credentials: 'include' })
                // .withUrl('discount-generator/discount-hub', {credentials: 'include'})
                .build();

            conn.on('ReceiveMessage', (message) => {
                setMessages((prevMessages) => [...prevMessages, message]);
            });

            conn.on('GenerationResponse', (response) => {
                setMessages((prevMessages) => [...prevMessages, `Generation response: ${response}`]);
            });

            conn.on('ReceiveCodes', (response) => {
                setCodes(response.codes);
                setPagination(response.pageInfo);
            });

            conn.on('UseCodeResponse', (response) => {
                setMessages((prevMessages) => [...prevMessages, `Use code response: ${response}`]);
            });

            conn.onclose(() => {
                console.log('Connection closed');
            });

            await conn.start();
            setConnection(conn);
        };

        establishConnection();

        return () => {
            if (connection) {
                connection.stop();
            }
        };
    }, []);

    const generateDiscountCodes = async (count, length) => {
        count = parseInt(count);
        length = parseInt(length);

        if (!count || !length) {
            return;
        }

        if (connection) {
            await connection.invoke('GenerateCodes', { count: count, length: length });
        }
    };

    const useDiscountCode = async (code) => {
        if (connection) {
            await connection.invoke('UseCode', code);
        }
    };

    const getDiscountCodes = async (pageNumber, pageSize) => {
        if (!pageNumber) {
            pageNumber = 1;
        }

        if (!pageSize) {
            pageSize = 10;
        }

        pageNumber = parseInt(pageNumber);
        pageSize = parseInt(pageSize);

        if (connection) {
            await connection.invoke('GetCodes', { pageNumber: pageNumber, pageSize: pageSize });
        }
    };

    return (
        <SignalRContext.Provider value={{ messages, codes, pagination, generateDiscountCodes, useDiscountCode, getDiscountCodes }}>
            {children}
        </SignalRContext.Provider>
    );
};