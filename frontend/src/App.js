import React, { useState } from 'react';
import './App.css';

function App() {
  const [name, setName] = useState('');
  const [data, setData] = useState(null);
  const [error, setError] = useState('');

  const fetchDemographics = async () => {
    setError('');
    try {
      const response = await fetch(`/demographics?name=${encodeURIComponent(name)}`);
      if (!response.ok) {
        setError(`Error: ${response.statusText}`);
        return;
      }
      const result = await response.json();
      setData(result);
    } catch (ex) {
      setError('Failed to fetch');
    }
  };

  return (
    <div className="container">
      <h1>Demographix</h1>

      <div className="input-group">
        <input
          type="text"
          placeholder="Enter name"
          value={name}
          onChange={e => setName(e.target.value)}
        />
        <button onClick={fetchDemographics} disabled={!name.trim()}>
          Search
        </button>
      </div>

      {error && <p className="error">{error}</p>}

      {data && (
        <div className="results">
          <p><strong>Name:</strong> {data.name}</p>
          <p><strong>Age:</strong> {data.age}</p>
          <p><strong>Gender:</strong> {data.gender} ({(data.genderProbability * 100).toFixed(2)}%)</p>
          <p><strong>Top Nationalities:</strong></p>
          <ul>
            {data.nationalities.map(n => (
              <li key={n.countryCode}>
                {n.countryName} ({(n.probability * 100).toFixed(2)}%)
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

export default App;
