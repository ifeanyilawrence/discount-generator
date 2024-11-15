import React, { useEffect, useState } from 'react';
import './Home.css';
import { useSignalR } from './SignalRContext';

const Home = () => {
    const { messages, codes, pagination, generateDiscountCodes, useDiscountCode, getDiscountCodes } = useSignalR();
    const [pageNumber, setPageNumber] = useState(pagination?.pageNumber || 1);

    useEffect(() => {
        getDiscountCodes(pageNumber, pagination?.pageSize);
    }, []);

    const handleGenerate = (count, length) => {
        generateDiscountCodes(count, length);
    };

    const handleUseCode = (code) => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        useDiscountCode(code);
    };

    const handlePagination = (direction) => {
        if (direction === 'next' && pageNumber < pagination?.totalPages) {
            setPageNumber(pageNumber + 1);
            getDiscountCodes(pageNumber + 1, pagination?.pageSize);
        } else if (direction === 'prev' && pageNumber > 1) {
            setPageNumber(pageNumber - 1);
            getDiscountCodes(pageNumber - 1, pagination?.pageSize);
        }
    };

  return (
      <div className="app">
          <div className="app-container">
              <h1>Discount Code Generator</h1>
              <div className="container">
                  <div className="button-container">
                      <div>
                          <input type="number" id="count" name="count" min="1" max="100" placeholder="Count"
                                 className="code-input" required/>
                          <input type="number" id="length" name="length" min="1" max="100" placeholder="Length"
                                 className="code-input" required/>
                          <button className="generate-button"
                                  onClick={() => handleGenerate(document.getElementById('count').value, document.getElementById('length').value)}>Generate
                              Discount Codes
                          </button>
                      </div>
                  </div>

                  <div className="table-container">
                      <table className="discount-table">
                          <thead>
                          <tr>
                              <th>#</th>
                              <th>Code</th>
                              <th>Used</th>
                          </tr>
                          </thead>
                          <tbody>
                          {codes?.map((code, index) => (
                              <tr key={index}>
                                  <td>{index + 1}</td>
                                  <td>{code.code}</td>
                                  <td>{code.isUsed ? 'Yes' : 'No'}</td>
                              </tr>
                          ))}
                          </tbody>
                      </table>

                      <div className="pagination">
                          <span>Page {pageNumber} of {pagination?.totalPages}</span>
                          <button className="pagination-button" id="prev-button"
                                  onClick={() => handlePagination('prev')} disabled={pageNumber === 1}>Previous
                          </button>
                          <button className="pagination-button" id="next-button"
                                  onClick={() => handlePagination('next')}
                                  disabled={pageNumber === pagination?.totalPages}>Next
                          </button>
                      </div>
                  </div>
              </div>

              <div className="divider"></div>

              <div className="code-input-container">
                  <input type="text" id="codeInput" placeholder="Enter code" className="code-input"/>
                  <button className="use-code-button"
                          onClick={() => handleUseCode(document.getElementById('codeInput').value)}>Use Code
                  </button>
              </div>

              <div className="messages">
                  {messages
                      .slice()
                      .reverse()
                      .map((msg, index) => (
                          <div className="message-item" key={index}>
                              <strong>{msg.sender}:</strong> {msg}
                              <div className="timestamp">{new Date().toLocaleString()}</div>
                          </div>
                      ))}
              </div>
          </div>
      </div>
  );
}

export default Home;