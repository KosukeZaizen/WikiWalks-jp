import React from 'react';
import { Route } from 'react-router';
import ScrollToTop from './components/ScrollToTop';
import Layout from './components/Layout';
import Home from './components/Home';
import Word from './components/Word';
import Category from './components/Category';
import Top from './components/Top';

export default () => (
    <Layout>
        <ScrollToTop>
            <Route exact path='/' component={Top} />
            <Route path='/category/:category' component={Category} />
            <Route path='/word/:wordId' component={Word} />
        </ScrollToTop>
    </Layout>
);
