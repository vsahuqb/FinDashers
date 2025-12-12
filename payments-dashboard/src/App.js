import React, { useState, useEffect } from 'react';
import './App.css';
import { applyTheme } from './styles/theme-config';
import Layout from './components/Layout';
import TabContainer from './components/TabContainer';
import TabPanel from './components/TabPanel';
import GridContainer from './components/GridContainer';
import ShowMoreButton from './components/ShowMoreButton';
import Widget from './components/Widget';
import KPICard from './components/KPICard';
import ChartContainer from './components/ChartContainer';
import Section from './components/Section';
import FilterBar from './components/FilterBar';
import BarChart from './components/charts/BarChart';
import GaugeChart from './components/charts/GaugeChart';
import LineChart from './components/charts/LineChart';
import HeatmapChart from './components/charts/HeatmapChart';
import FunnelChart from './components/charts/FunnelChart';
import SankeyChart from './components/charts/SankeyChart';
import RiskRingChart from './components/charts/RiskRingChart';
import { DataProvider } from './context/DataContext';
import { PreferencesProvider, usePreferences } from './context/PreferencesContext';

const AppContent = () => {
  const [showMoreAnalytics, setShowMoreAnalytics] = useState(false);
  const { preferences, setFilters, resetPreferences } = usePreferences();

  // Apply theme on component mount
  useEffect(() => {
    applyTheme();
  }, []);

  const handleFiltersChange = (newFilters) => {
    setFilters(newFilters);
    console.log('Filters changed:', newFilters);
  };

  return (
    <DataProvider>
      <div className="App">
        <Layout>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
            <FilterBar onFiltersChange={handleFiltersChange} initialFilters={preferences.filters} />
            <button 
              onClick={resetPreferences}
              style={{
                padding: '8px 16px',
                background: 'rgba(79, 70, 229, 0.1)',
                border: '1px solid rgba(79, 70, 229, 0.3)',
                borderRadius: '8px',
                color: '#4f46e5',
                cursor: 'pointer'
              }}
            >
              Reset Layout
            </button>
          </div>
          <TabContainer>
            <TabPanel tabId="overview">
              <Section>
                <GridContainer className="grid-1">
                  <div className="grid-item full-width">
                    <KPICard />
                  </div>
                </GridContainer>
              </Section>
              
              <Section title="Key Metrics" collapsible={true} sectionId="overview-metrics">
                <GridContainer className="grid-2">
                  <Widget 
                    id="success-rate-gauge" 
                    title="Success Rate"
                    minimizable={true}
                    maximizable={true}
                    closable={true}
                  >
                    <ChartContainer skeletonType="gauge">
                      <GaugeChart />
                    </ChartContainer>
                  </Widget>
                  <Widget 
                    id="risk-components" 
                    title="Risk Components"
                    minimizable={true}
                    maximizable={true}
                    closable={true}
                  >
                    <ChartContainer>
                      <RiskRingChart />
                    </ChartContainer>
                  </Widget>
                </GridContainer>
              </Section>
            </TabPanel>
            
            <TabPanel tabId="analytics">
              {/* Core Analytics - Always Visible */}
              <Section 
                title="Payment Flow Analysis" 
                collapsible={true} 
                sectionId="payment-flow"
              >
                <GridContainer className="grid-2">
                  <Widget 
                    id="transaction-funnel" 
                    title="Transaction Funnel"
                    minimizable={true}
                    maximizable={true}
                    closable={false}
                  >
                    <ChartContainer>
                      <FunnelChart />
                    </ChartContainer>
                  </Widget>
                  <Widget 
                    id="payment-flow" 
                    title="Payment Flow"
                    minimizable={true}
                    maximizable={true}
                    closable={false}
                  >
                    <ChartContainer>
                      <SankeyChart />
                    </ChartContainer>
                  </Widget>
                </GridContainer>
              </Section>
              
              {/* Show More Button */}
              <ShowMoreButton 
                showMore={showMoreAnalytics}
                onToggle={() => setShowMoreAnalytics(!showMoreAnalytics)}
                moreCount={3}
              />
              
              {/* Additional Analytics - Progressive Disclosure */}
              {showMoreAnalytics && (
                <>
                  <Section 
                    title="Risk & Health Metrics" 
                    collapsible={true} 
                    sectionId="risk-health"
                  >
                    <GridContainer className="grid-2">
                      <Widget 
                        id="risk-analysis" 
                        title="Risk Analysis"
                        minimizable={true}
                        maximizable={true}
                        closable={true}
                        defaultMinimized={false}
                      >
                        <ChartContainer>
                          <RiskRingChart />
                        </ChartContainer>
                      </Widget>
                      <Widget 
                        id="health-gauge" 
                        title="Health Gauge"
                        minimizable={true}
                        maximizable={true}
                        closable={true}
                      >
                        <ChartContainer>
                          <GaugeChart />
                        </ChartContainer>
                      </Widget>
                    </GridContainer>
                  </Section>
                  
                  <Section 
                    title="Failure Analysis" 
                    collapsible={true} 
                    sectionId="failure-analysis"
                  >
                    <GridContainer className="grid-2">
                      <Widget 
                        id="failure-heatmap" 
                        title="Failure Heatmap"
                        minimizable={true}
                        maximizable={true}
                        closable={true}
                      >
                        <ChartContainer>
                          <HeatmapChart />
                        </ChartContainer>
                      </Widget>
                      <Widget 
                        id="failure-trends" 
                        title="Failure Trends"
                        minimizable={true}
                        maximizable={true}
                        closable={true}
                      >
                        <ChartContainer skeletonType="line">
                          <LineChart />
                        </ChartContainer>
                      </Widget>
                    </GridContainer>
                  </Section>
                  
                  <Section 
                    title="Payment Method Performance" 
                    collapsible={true} 
                    sectionId="payment-methods"
                  >
                    <GridContainer className="grid-1">
                      <Widget 
                        id="payment-methods" 
                        title="Payment Methods"
                        minimizable={true}
                        maximizable={true}
                        closable={true}
                      >
                        <ChartContainer className="grid-item full-width" skeletonType="bar">
                          <BarChart />
                        </ChartContainer>
                      </Widget>
                    </GridContainer>
                  </Section>
                </>
              )}
            </TabPanel>
            
            <TabPanel tabId="search">
              <Section title="Search">
                <div style={{ padding: '40px', textAlign: 'center', color: 'white' }}>
                  <h3>Search functionality coming soon...</h3>
                  <p>Natural language search will be implemented here.</p>
                </div>
              </Section>
            </TabPanel>
            
            <TabPanel tabId="settings">
              <Section title="Settings">
                <div style={{ padding: '40px', textAlign: 'center', color: 'white' }}>
                  <h3>Dashboard settings</h3>
                  <p>Configuration options will be available here.</p>
                </div>
              </Section>
            </TabPanel>
          </TabContainer>
        </Layout>
      </div>
    </DataProvider>
  );
};

function App() {
  return (
    <PreferencesProvider>
      <AppContent />
    </PreferencesProvider>
  );
}

export default App;
